using Microsoft.CodeAnalysis;
using OpenH2.Serialization.Layout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace OpenH2.Serialization.Materialization
{
    internal class WellKnown
    {
        public Dictionary<ITypeSymbol, string> PrimitiveReaders { get; }
        public Dictionary<ITypeSymbol, int> SizeOf { get; }

        public WellKnown(Compilation compilation, TypeDiscoverer typeDiscoverer)
        {
            PrimitiveReaders = TransformDictionary(compilation, ReflectionPrimitiveReaders);
            SizeOf = TransformDictionary(compilation, ReflectionSizeOf);

            var fixedLengthAttr = compilation.GetTypeSymbol<FixedLengthAttribute>();

            foreach(var t in typeDiscoverer.Types)
            {
                var s = compilation.GetSemanticModel(t.SyntaxTree).GetDeclaredSymbol(t) as ITypeSymbol;

                var attrs = s.GetAttributes();

                foreach(var a in attrs)
                {
                    if(SymbolEqualityComparer.Default.Equals(a.AttributeClass, fixedLengthAttr))
                    {
                        var length = (int)a.ConstructorArguments[0].Value;
                        SizeOf[s] = length;
                    }
                }

                var materializerAttr = compilation.GetTypeSymbol<PrimitiveValueMaterializerAttribute>();
                var methods = s.GetMembers().OfType<IMethodSymbol>();

                foreach(var m in methods)
                {
                    var mattrs = m.GetAttributes();

                    if(mattrs.Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, materializerAttr)))
                    {
                        // TODO: check parameter(s)?

                        PrimitiveReaders[m.ReturnType] = m.Name;
                    }
                }
            }
        }

        private Dictionary<ITypeSymbol, T> TransformDictionary<T>(Compilation compilation, Dictionary<Type, T> inDict)
        {
            var dict = new Dictionary<ITypeSymbol, T>();

            foreach (var kv in inDict)
            {
                var symbol = compilation.GetTypeSymbol(kv.Key);

                if (symbol != null)
                {
                    dict.Add(symbol, kv.Value);
                }
            }

            return dict;
        }

        private static Dictionary<Type, string> ReflectionPrimitiveReaders = new Dictionary<Type, string>
        {
            { typeof(byte), nameof(SpanByteExtensions.ReadByteAt) },
            { typeof(short), nameof(SpanByteExtensions.ReadInt16At) },
            { typeof(ushort), nameof(SpanByteExtensions.ReadUInt16At) },
            { typeof(int), nameof(SpanByteExtensions.ReadInt32At) },
            { typeof(uint), nameof(SpanByteExtensions.ReadUInt32At) },
            { typeof(float), nameof(SpanByteExtensions.ReadFloatAt) },
            { typeof(string), nameof(SpanByteExtensions.ReadStringFrom) },
            { typeof(Vector2), nameof(SpanByteExtensions.ReadVec2At) },
            { typeof(Vector3), nameof(SpanByteExtensions.ReadVec3At) },
            { typeof(Vector4), nameof(SpanByteExtensions.ReadVec4At) },
            { typeof(Quaternion), nameof(SpanByteExtensions.ReadQuaternionAt) },
            { typeof(Matrix4x4), nameof(SpanByteExtensions.ReadMatrix4x4At) },

            //{ typeof(TagRef), nameof(SpanByteExtensions.ReadTagRefAt)) },
            //{ typeof(TagRef<>), nameof(SpanByteExtensions.ReadTagRefAt)) },
            //{ typeof(InternedString), nameof(SpanByteExtensions.ReadInternedStringAt)) },
        };

        private Dictionary<Type, int> ReflectionSizeOf = new Dictionary<Type, int>
        {
            { typeof(Vector2), Unsafe.SizeOf<Vector2>() },
            { typeof(Vector3), Unsafe.SizeOf<Vector3>() },
            { typeof(Vector4), Unsafe.SizeOf<Vector4>() },
            { typeof(Quaternion), Unsafe.SizeOf<Quaternion>() },
            { typeof(Matrix4x4), Unsafe.SizeOf<Matrix4x4>() },
        };
    }
}
