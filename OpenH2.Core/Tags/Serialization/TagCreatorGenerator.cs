using OpenH2.Core.Extensions;
using OpenH2.Core.Offsets;
using OpenH2.Core.Tags.Layout;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.Serialization;

namespace OpenH2.Core.Tags.Serialization
{
    public delegate object TagCreator(Span<byte> data, int internalOffsetMagic, int startAt = 0);
    public delegate T TagCreator<T>(Span<byte> data, int internalOffsetMagic, int startAt = 0);

    public class TagCreatorGenerator
    {
        private static Type[] arguments = new[] { typeof(Span<byte>), typeof(int), typeof(int) };
        private static Dictionary<Type, TagCreator> cachedTagCreatorDelegates = new Dictionary<Type, TagCreator>();


        private Func<Type, SerializerBuilderContext> builderWrapperFactory;

        
        public TagCreatorGenerator(ModuleBuilder module = null)
        {
            if(module == null)
            {
                var assyBuilder = AssemblyBuilder.DefineDynamicAssembly(
                    new AssemblyName("TagSerialization"), 
                    AssemblyBuilderAccess.Run);

                module = assyBuilder.DefineDynamicModule(assyBuilder.GetName().Name);
            }

            builderWrapperFactory = tagType =>
            {
                var type = module.DefineType($"{tagType.Name}Serializer", TypeAttributes.Public);

                var builder = type.DefineMethod("Read" + tagType.Name,
                    MethodAttributes.Public | MethodAttributes.Static,
                    typeof(object),
                    arguments);

                var wrapper = new SerializerBuilderContext()
                {
                    TypeBuilder = type,
                    MethodBuilder = builder,
                };

                return wrapper;
            };
        }

        public TagCreator<T> GetTagCreator<T>()
        {
            var tagType = typeof(T);

            var creator = GetTagCreator(tagType);

            return new TagCreator<T>((Span<byte> s, int i, int o) => (T)creator(s, i, o));
        }

        public TagCreator GetTagCreator(Type tagType)
        {
            if (cachedTagCreatorDelegates.TryGetValue(tagType, out var deleg))
            {
                return deleg;
            }

            var context = builderWrapperFactory(tagType);

            GenerateTagCreator(tagType, context);

            var createdType = context.TypeBuilder.CreateTypeInfo().AsType();

            var method = createdType.GetMethod(context.MethodBuilder.Name);

            return (TagCreator)method.CreateDelegate(typeof(TagCreator));
        }

        private MethodBuilder GenerateNestedTagCreatorMethod(Type nestedTagType, SerializerBuilderContext context)
        {
            var name = nestedTagType.FullName
              .Substring(nestedTagType.FullName.LastIndexOf('.') + 1)
              .Replace("+", "_");

            var methodBuilder = context.TypeBuilder.DefineMethod("Read" + name,
                MethodAttributes.Public | MethodAttributes.Static,
                typeof(object),
                arguments);

            var newContext = new SerializerBuilderContext()
            {
                TypeBuilder = context.TypeBuilder,
                MethodBuilder = methodBuilder
            };

            GenerateTagCreator(nestedTagType, newContext);

            return methodBuilder;
        }

        private void GenerateTagCreator(Type tagType, SerializerBuilderContext context)
        {
            var gen = context.MethodBuilder.GetILGenerator();
            var internalPropCreators = new Dictionary<TagProperty, MethodInfo>();

#if DEBUG
            var debugWrite = typeof(Debug).GetMethod("WriteLine", new[] { typeof(object) });
#endif

            /// public Tag TagCreator(uint id, Span<byte> data)
            /// {
            ///     var t= new Tag();
            /// 
            ///     // foreach prop
            ///     var val = data.ReadXAt(prop.Offset);
            ///     t.Prop = val;
            /// 
            ///     // foreach refprop
            ///     var result = new p[];
            ///     for(cao)
            ///         result[i] = new p();
            ///         
            ///     t.Prop = result;
            /// 
            ///     return t;
            /// };

            var props = TagTypeMetadataProvider.GetProperties(tagType);

            var tagLocal = gen.DeclareLocal(tagType);
            LocalBuilder caoLocal = null;

            if (props.Any(p => p.LayoutAttribute is InternalReferenceValueAttribute))
            {
                Debug.WriteLine("Generating Cao lookup for reference props");

                var caoLookupType = typeof(Dictionary<int, CountAndOffset>);

                caoLocal = gen.DeclareLocal(caoLookupType);

                gen.Emit(OpCodes.Newobj, caoLookupType.GetConstructor(new Type[] { }));
                gen.Emit(OpCodes.Stloc, caoLocal);
            }

            gen.Emit(OpCodes.Ldtoken, tagType);
            gen.Emit(OpCodes.Call, MI.SystemType.GetTypeFromHandle);
            gen.Emit(OpCodes.Call, MI.Runtime.GetUninitializedObject);

            // If it's a class, we need to cast to store in local
            // If it's a struct, we need to unbox 
            if(tagType.IsClass)
            {
                gen.Emit(OpCodes.Castclass, tagType);
            }
            else
            {
                gen.Emit(OpCodes.Unbox_Any, tagType);
            }

            gen.Emit(OpCodes.Stloc, tagLocal);

            // Do first pass over "header" reagion
            foreach (var prop in props)
            {
                switch (prop.LayoutAttribute)
                {
                    case PrimitiveValueAttribute prim:
                        GeneratePrimitiveProperty(gen, tagLocal, prop, tagType);
                        break;

                    case InternalReferenceValueAttribute reference:
                        var propType = prop.Type.IsArray ? prop.Type.GetElementType() : prop.Type;
                        internalPropCreators[prop] = GenerateNestedTagCreatorMethod(propType, context);
                        GenerateReferenceProperty(gen, caoLocal, prop);
                        break;
                }
            }

            // Do subsequent passes for internal references
            foreach (var prop in props.Where(p => p.LayoutAttribute is InternalReferenceValueAttribute))
            {
                var creator = internalPropCreators[prop];
                GenerateInternalReferenceArrayReader(gen, tagLocal, caoLocal, prop, tagType, creator);
            }

            gen.Emit(OpCodes.Ldloc, tagLocal);

            // If it's a struct, we need to box, as we return object
            if(tagType.IsClass == false)
            {
                gen.Emit(OpCodes.Box, tagType);
            }

            gen.Emit(OpCodes.Ret);

            
        }

        private static void GenerateInternalReferenceArrayReader(ILGenerator gen, LocalBuilder tagLocal, LocalBuilder caoLocal, TagProperty prop, Type tagType, MethodInfo creator)
        {
            if (prop.Type.BaseType != typeof(Array))
            {
                throw new Exception("Internal references must be array properties");
            }

            var cao = gen.DeclareLocal(typeof(CountAndOffset));

            gen.Emit(OpCodes.Ldloc, caoLocal);
            gen.Emit(OpCodes.Ldc_I4, prop.LayoutAttribute.Offset); // cao lookup key
            gen.Emit(OpCodes.Callvirt, MI.Cao.IntDictLookup); // Cao is on the stack
            gen.Emit(OpCodes.Stloc, cao); // store cao

            var count = gen.DeclareLocal(typeof(int));
            var offset = gen.DeclareLocal(typeof(int));

            gen.Emit(OpCodes.Ldloc, cao);
            gen.Emit(OpCodes.Callvirt, MI.Cao.OffsetGetter); // Get cao IOffset
            gen.Emit(OpCodes.Callvirt, MI.Offset.IOffsetValue); // Get IOffset value
            gen.Emit(OpCodes.Stloc, offset);

            gen.Emit(OpCodes.Ldloc, cao);
            gen.Emit(OpCodes.Callvirt, MI.Cao.CountGetter); // Get Cao count
            gen.Emit(OpCodes.Stloc, count);

            var i = gen.DeclareLocal(typeof(int));
            var loop = gen.DefineLabel();
            var loopCheck = gen.DefineLabel();

            // Create item[] result item
            var result = gen.DeclareLocal(prop.Type);
            var resultCtor = prop.Type.GetConstructor(new[] { typeof(int) });
            gen.Emit(OpCodes.Ldloc, count);
            gen.Emit(OpCodes.Newobj, resultCtor);
            gen.Emit(OpCodes.Stloc, result);

            // var i = 0; goto loopcheck
            gen.Emit(OpCodes.Ldc_I4_0);
            gen.Emit(OpCodes.Stloc, i);
            gen.Emit(OpCodes.Br, loopCheck);
            gen.MarkLabel(loop);

            // loop body
            {
                gen.Emit(OpCodes.Ldloc, result);
                gen.Emit(OpCodes.Ldloc, i);

                var elemType = prop.Type.GetElementType();
                var typeLength = TagTypeMetadataProvider.GetFixedLength(elemType);

                gen.Emit(OpCodes.Ldarg_0); // load span

                gen.Emit(OpCodes.Ldarg_1); // load magic

                // offset + (i * length)
                gen.Emit(OpCodes.Ldloc, offset);
                gen.Emit(OpCodes.Ldloc, i);
                gen.Emit(OpCodes.Ldc_I4, typeLength);
                gen.Emit(OpCodes.Mul); 
                gen.Emit(OpCodes.Add);// Load item start

                gen.Emit(OpCodes.Call, creator);

                if(elemType.IsValueType)
                {
                    gen.Emit(OpCodes.Unbox_Any, elemType);
                    gen.Emit(OpCodes.Stelem, elemType);
                }
                else
                {
                    gen.Emit(OpCodes.Stelem_Ref);
                }
            }

            // i++
            gen.Emit(OpCodes.Ldloc, i);
            gen.Emit(OpCodes.Ldc_I4_1);
            gen.Emit(OpCodes.Add);
            gen.Emit(OpCodes.Stloc, i);

            // if ( i < count) goto loop
            gen.MarkLabel(loopCheck);
            gen.Emit(OpCodes.Ldloc, i);
            gen.Emit(OpCodes.Ldloc, count);
            gen.Emit(OpCodes.Clt);
            gen.Emit(OpCodes.Brtrue, loop);

            if(tagType.IsClass)
            {
                gen.Emit(OpCodes.Ldloc, tagLocal);
            }
            else
            {
                // Value type requires loading address instead of value
                gen.Emit(OpCodes.Ldloca, tagLocal);
            }
            
            gen.Emit(OpCodes.Ldloc, result);
            gen.Emit(OpCodes.Call, prop.Setter);
        }

        /// <summary>
        /// Generate Count and Offset reading, create CAO object and add to Dict<int,cao> in caoLocal
        /// </summary>
        /// <param name="gen"></param>
        /// <param name="caoLocal"></param>
        /// <param name="prop"></param>
        private static void GenerateReferenceProperty(ILGenerator gen, LocalBuilder caoLocal, TagProperty prop)
        {
            gen.Emit(OpCodes.Ldloc, caoLocal); // Load Cao dict for later

            gen.Emit(OpCodes.Ldc_I4, prop.LayoutAttribute.Offset); // Load Cao dict key for later

            gen.Emit(OpCodes.Ldarg_0); // Load Span<byte> onto evalstack

            gen.Emit(OpCodes.Ldarg_2); // Load start offset
            gen.Emit(OpCodes.Ldc_I4, prop.LayoutAttribute.Offset); // Load offset onto evalstack
            gen.Emit(OpCodes.Add); // start + offset

            gen.Emit(OpCodes.Ldarg_1); // load magic

            gen.Emit(OpCodes.Call, MI.SpanByte.ReadMetaCaoAt); // consume count from above and InternalOffset

            gen.Emit(OpCodes.Callvirt, MI.Cao.IntDictAddMethod); // Push Prop layout offset and Cao
        }

        private static void GeneratePrimitiveProperty(ILGenerator gen, LocalBuilder tagLocal, TagProperty prop, Type tagType)
        {
            var type = prop.Type;

            if (type.IsEnum)
            {
                type = Enum.GetUnderlyingType(type);
            }

            if (PrimitiveSpanReaders.TryGetValue(type, out var readerMethod))
            {
                // Load tag onto evalstack for later
                if(tagType.IsClass)
                {
                    gen.Emit(OpCodes.Ldloc, tagLocal);
                }
                else
                {
                    // Must load the address when value type
                    gen.Emit(OpCodes.Ldloca, tagLocal);
                }

                gen.Emit(OpCodes.Ldarg_0); // Load Span<byte> onto evalstack

                gen.Emit(OpCodes.Ldarg_2);
                gen.Emit(OpCodes.Ldc_I4, prop.LayoutAttribute.Offset); // Load offset onto evalstack
                gen.Emit(OpCodes.Add);

                gen.Emit(OpCodes.Call, readerMethod); // Consume span and offset

                // Consume tag and returned value args, set tag val
                gen.Emit(OpCodes.Call, prop.Setter);
            }
        }


        internal class SerializerBuilderContext
        {
            public TypeBuilder TypeBuilder;
            public MethodBuilder MethodBuilder;
        }

        private static Dictionary<Type, MethodInfo> PrimitiveSpanReaders = new Dictionary<Type, MethodInfo>
        {
            { typeof(short), typeof(SpanByteExtensions).GetMethod(nameof(SpanByteExtensions.ReadInt16At)) },
            { typeof(ushort), typeof(SpanByteExtensions).GetMethod(nameof(SpanByteExtensions.ReadUInt16At)) },
            { typeof(int), MI.SpanByte.ReadInt32At},
            { typeof(uint), typeof(SpanByteExtensions).GetMethod(nameof(SpanByteExtensions.ReadUInt32At)) },
            { typeof(float), typeof(SpanByteExtensions).GetMethod(nameof(SpanByteExtensions.ReadFloatAt)) },
        };

        private static class MI
        {
            public static class Runtime
            {
                public static MethodInfo GetUninitializedObject = typeof(FormatterServices)
                    .GetMethod(nameof(FormatterServices.GetUninitializedObject));
            }

            public static class SystemType
            {
                public static MethodInfo GetTypeFromHandle = typeof(Type)
                    .GetMethod(nameof(Type.GetTypeFromHandle));
            }

            public static class SpanByte
            {
                public static MethodInfo ReadInt32At = typeof(SpanByteExtensions)
                    .GetMethod(nameof(SpanByteExtensions.ReadInt32At));

                public static MethodInfo ReadMetaCaoAt = typeof(SpanByteExtensions)
                    .GetMethod(nameof(SpanByteExtensions.ReadMetaCaoAt), new[] { typeof(Span<byte>), typeof(int), typeof(int) });

                public static MethodInfo Slice = typeof(Span<byte>)
                    .GetMethod(nameof(Span<byte>.Slice), new[] { typeof(int), typeof(int) });
            }

            public static class Cao
            {
                public static ConstructorInfo Ctor = typeof(CountAndOffset)
                    .GetConstructor(new[] { typeof(int), typeof(IOffset) });

                public static MethodInfo OffsetGetter = typeof(CountAndOffset)
                   .GetProperty(nameof(CountAndOffset.Offset)).GetGetMethod();

                public static MethodInfo CountGetter = typeof(CountAndOffset)
                    .GetProperty(nameof(CountAndOffset.Count)).GetGetMethod();

                public static MethodInfo IntDictAddMethod = typeof(Dictionary<int, CountAndOffset>)
                    .GetMethod("Add", new[] { typeof(int), typeof(CountAndOffset) });

                public static MethodInfo IntDictLookup = typeof(Dictionary<int, CountAndOffset>)
                    .GetMethod("get_Item", new[] { typeof(int) });
            }

            public static class Offset
            {
                public static ConstructorInfo TagInternalOffsetCtor = typeof(TagInternalOffset)
                    .GetConstructor(new[] { typeof(int), typeof(int) });

                public static MethodInfo IOffsetValue = typeof(IOffset)
                    .GetProperty(nameof(IOffset.Value)).GetGetMethod();
            }
        }
    }
}
