using OpenH2.Core.Extensions;
using OpenH2.Core.Offsets;
using OpenH2.Core.Representations;
using OpenH2.Core.Tags.Layout;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace OpenH2.Core.Tags.Processors
{
    public class TagCreatorGenerator
    {
        public delegate T TagCreator<T>(uint id, Span<byte> data, TagIndexEntry index);

        private static Type[] arguments = new[] { typeof(uint), typeof(Span<byte>), typeof(TagIndexEntry) };
        
        public TagCreator<T> GenerateTagCreator<T>() where T : BaseTag
        {
            var tagType = typeof(T);

            var builder = new DynamicMethod("Read" + tagType.Name, tagType, arguments);

            var wrapper = new MethodBuilderWrapper<T>()
            {
                generator = builder.GetILGenerator(),
                getDelegate = () => (TagCreator<T>)builder.CreateDelegate(typeof(TagCreator<T>))
            };

            return GenerateTagCreator(wrapper);
        }

        public TagCreator<T> GenerateTagCreator<T>(TypeBuilder type) where T: BaseTag
        {
            var tagType = typeof(T);

            var builder = type.DefineMethod("Read" + tagType.Name,
                MethodAttributes.Public, tagType, arguments);

            var wrapper = new MethodBuilderWrapper<T>()
            {
                generator = builder.GetILGenerator(),
                getDelegate = () => (TagCreator<T>)builder.CreateDelegate(typeof(TagCreator<T>))
            };

            return GenerateTagCreator(wrapper);
        }

        private TagCreator<T> GenerateTagCreator<T>(MethodBuilderWrapper<T> builder) where T : BaseTag
        {
            var gen = builder.generator;

#if DEBUG
            var debugWrite = typeof(Debug).GetMethod("WriteLine", new[] { typeof(object) });
#endif

            var tagType = typeof(T);

            /// public Tag TagCreator(uint id, Span<byte> data)
            /// {
            ///     var t= new Tag();
            /// 
            ///     // foreach prop
            ///     var val = data.ReadXAt(prop.Offset);
            ///     t.Prop = val;
            /// 
            ///     return t;
            /// };

            var props = TagTypeMetadataProvider.GetProperties(typeof(T));

            var tagLocal = gen.DeclareLocal(tagType);
            LocalBuilder caoLocal = null;

            if (props.Any(p => p.LayoutAttribute is InternalReferenceValueAttribute))
            {
                Debug.WriteLine("Generating Cao lookup for reference props");

                var caoLookupType = typeof(Dictionary<int, CountAndOffset>);

                caoLocal = gen.DeclareLocal(caoLookupType);

                gen.Emit(OpCodes.Newobj, caoLookupType.GetConstructor(new Type[] { }));
                gen.Emit(OpCodes.Stloc, caoLocal.LocalIndex);
            }

            gen.Emit(OpCodes.Ldarg_0);
            gen.Emit(OpCodes.Newobj, tagType.GetConstructor(new Type[] { typeof(uint) }));
            gen.Emit(OpCodes.Stloc, tagLocal.LocalIndex);

            // Do first pass over "header" reagion
            foreach (var prop in props)
            {
                switch (prop.LayoutAttribute)
                {
                    case PrimitiveValueAttribute prim:
                        GeneratePrimitiveProperty(gen, tagLocal, prop);
                        break;

                    case InternalReferenceValueAttribute reference:
                        //GenerateReferenceProperty(gen, caoLocal, prop);
                        break;
                }
            }

            // Do subsequent passes for internal references
            foreach (var prop in props.Where(p => p.LayoutAttribute is InternalReferenceValueAttribute))
            {
                //GenerateInternalReferenceArrayReader(gen, tagLocal, caoLocal, prop);
            }

            gen.Emit(OpCodes.Ldloc, tagLocal.LocalIndex);
            gen.Emit(OpCodes.Ret);

            return builder.getDelegate();
        }

        private void GenerateInternalReferenceArrayReader(ILGenerator gen, LocalBuilder tagLocal, LocalBuilder caoLocal, TagProperty prop)
        {
            if (prop.Type.BaseType != typeof(Array))
            {
                throw new Exception("Internal references must be array properties");
            }

            var cao = gen.DeclareLocal(typeof(CountAndOffset));

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

                var ctor = prop.Type.GetElementType().GetConstructor(new Type[] { });

                gen.Emit(OpCodes.Newobj, ctor);

                gen.Emit(OpCodes.Stelem_Ref);
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
        }

        /// <summary>
        /// Generate Count and Offset reading, create CAO object and add to Dict<int,cao> in caoLocal
        /// </summary>
        /// <param name="gen"></param>
        /// <param name="caoLocal"></param>
        /// <param name="prop"></param>
        private void GenerateReferenceProperty(ILGenerator gen, LocalBuilder caoLocal, TagProperty prop)
        {
            gen.Emit(OpCodes.Ldc_I4, prop.LayoutAttribute.Offset);

            gen.Emit(OpCodes.Ldarg_1); // Load Span<byte> onto evalstack

            gen.Emit(OpCodes.Ldc_I4, prop.LayoutAttribute.Offset); // Load offset onto evalstack

            gen.Emit(OpCodes.Call, MI.SpanByte.ReadInt32At); // Consume span and offset, load count

            gen.Emit(OpCodes.Ldarg_2); // load tag index

            gen.Emit(OpCodes.Ldarg_1); // Load Span<byte> onto evalstack

            gen.Emit(OpCodes.Ldc_I4, prop.LayoutAttribute.Offset + 4); // Load offset onto evalstack

            gen.Emit(OpCodes.Call, MI.SpanByte.ReadInt32At); // Consume span and offset, load offset value onto evalstack

            gen.Emit(OpCodes.Newobj, MI.Offset.TagInternalOffsetCtor); // consume tag index and offset value, loading InternalOffset

            gen.Emit(OpCodes.Newobj, MI.Cao.Ctor); // consume count from above and InternalOffset

            gen.Emit(OpCodes.Callvirt, MI.Cao.IntDictAddMethod); // Push Prop layout offset and Cao
        }

        private void GeneratePrimitiveProperty(ILGenerator gen, LocalBuilder tagLocal, TagProperty prop)
        {
            var type = prop.Type;

            if (type.IsEnum)
            {
                type = Enum.GetUnderlyingType(type);
            }

            if (PrimitiveSpanReaders.TryGetValue(type, out var readerMethod))
            {
                gen.Emit(OpCodes.Ldloc, tagLocal.LocalIndex); // Load tag onto evalstack for later

                gen.Emit(OpCodes.Ldarg_1); // Load Span<byte> onto evalstack

                gen.Emit(OpCodes.Ldc_I4, prop.LayoutAttribute.Offset); // Load offset onto evalstack

                gen.Emit(OpCodes.Call, readerMethod); // Consume span and offset

                gen.Emit(OpCodes.Callvirt, prop.Setter); // Consume tag and returned value args, set tag val
            }
        }


        private class MethodBuilderWrapper<T>
        {
            public ILGenerator generator;
            public Func<TagCreator<T>> getDelegate;
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
            public static class SpanByte
            {
                public static MethodInfo ReadInt32At = typeof(SpanByteExtensions)
                    .GetMethod(nameof(SpanByteExtensions.ReadInt32At));
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
                    .GetConstructor(new[] { typeof(TagIndexEntry), typeof(int) });

                public static MethodInfo IOffsetValue = typeof(IOffset)
                    .GetProperty(nameof(IOffset.Value)).GetGetMethod();
            }
        }
    }
}
