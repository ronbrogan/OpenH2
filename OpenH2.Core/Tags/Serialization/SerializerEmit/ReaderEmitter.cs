using OpenH2.Core.Offsets;
using OpenH2.Core.Tags.Layout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace OpenH2.Core.Tags.Serialization.SerializerEmit
{
    internal static class ReaderEmitter
    {
        private static MethodInfo GenerateNestedTagCreatorMethod(Type nestedTagType, SerializerEmitContext context)
        {
            var newContext = context.GetNestedContext(nestedTagType);

            GenerateTagCreator(nestedTagType, newContext);

            return newContext.GetMethodInfo();
        }

        public static void GenerateTagCreator(Type tagType, SerializerEmitContext context)
        {
            var gen = context.MethodIL;
            var internalPropCreators = new Dictionary<TagProperty, MethodInfo>();

            var props = TagTypeMetadataProvider.GetProperties(tagType);

            var tagLocal = gen.DeclareLocal(tagType);
            LocalBuilder caoLocal = null;

            if (props.Any(p => p.LayoutAttribute is InternalReferenceValueAttribute))
            {
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
            if (tagType.IsClass)
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

                    case PrimitiveArrayAttribute arr:
                        GeneratePrimitiveArrayProperty(gen, tagLocal, prop, tagType, arr);
                        break;

                    case InternalReferenceValueAttribute reference:
                        var propType = prop.Type.IsArray ? prop.Type.GetElementType() : prop.Type;
                        // Ensure that any nested type creators are pre-created
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
            if (tagType.IsClass == false)
            {
                gen.Emit(OpCodes.Box, tagType);
            }

            gen.Emit(OpCodes.Ret);
        }

        private static void GeneratePrimitiveArrayProperty(ILGenerator gen, LocalBuilder tagLocal, TagProperty prop, Type tagType, PrimitiveArrayAttribute arr)
        {
            if (prop.Type.BaseType != typeof(Array))
            {
                throw new Exception("PrimitiveArray properties must be array types");
            }

            var elemType = prop.Type.GetElementType();

            if (elemType.IsEnum)
            {
                elemType = Enum.GetUnderlyingType(elemType);
            }

            if (MI.PrimitiveSpanReaders.TryGetValue(elemType, out var readerMethod) == false)
            {
                return;
            }

            if(MI.PrimitiveSizes.TryGetValue(elemType, out var typeLength) == false)
            {
                return;
            }

            var i = gen.DeclareLocal(typeof(int));
            var loop = gen.DefineLabel();
            var loopCheck = gen.DefineLabel();

            // Create item[] result item
            var result = gen.DeclareLocal(prop.Type);
            var resultCtor = prop.Type.GetConstructor(new[] { typeof(int) });
            gen.Emit(OpCodes.Ldc_I4, arr.Count);
            gen.Emit(OpCodes.Newobj, resultCtor);
            gen.Emit(OpCodes.Stloc, result);

            // var i = 0; goto loopcheck
            gen.Emit(OpCodes.Ldc_I4_0);
            gen.Emit(OpCodes.Stloc, i);
            gen.Emit(OpCodes.Br, loopCheck);
            gen.MarkLabel(loop);

            // loop body
            {
                // prep array and index of end of method setting
                gen.Emit(OpCodes.Ldloc, result);
                gen.Emit(OpCodes.Ldloc, i);

                // Load Span<byte> onto evalstack
                gen.Emit(OpCodes.Ldarg_0);

                // Load item start
                // start + offset + (i * length)
                gen.Emit(OpCodes.Ldloc, i);
                gen.Emit(OpCodes.Ldc_I4, typeLength);
                gen.Emit(OpCodes.Mul);
                gen.Emit(OpCodes.Ldc_I4, arr.Offset);
                gen.Emit(OpCodes.Add);
                gen.Emit(OpCodes.Ldarg_2); // Load subtag start
                gen.Emit(OpCodes.Add);

                // Consume span and offset
                gen.Emit(OpCodes.Call, readerMethod);

                // Add to array
                if (elemType.IsValueType)
                {
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
            gen.Emit(OpCodes.Ldc_I4, arr.Count);
            gen.Emit(OpCodes.Clt);
            gen.Emit(OpCodes.Brtrue, loop);

            if (tagType.IsClass)
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

                if (elemType.IsValueType)
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

            if (tagType.IsClass)
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

            if (MI.PrimitiveSpanReaders.TryGetValue(type, out var readerMethod))
            {
                // Load tag onto evalstack for later
                if (tagType.IsClass)
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
    }
}
