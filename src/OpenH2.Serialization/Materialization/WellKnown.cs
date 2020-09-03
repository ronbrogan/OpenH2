using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenH2.Serialization.Layout;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace OpenH2.Serialization.Materialization
{
    internal delegate InvocationExpressionSyntax PrimitiveReader(ExpressionSyntax data, params ArgumentSyntax[] otherArgs);
    internal class WellKnown
    {
        public Dictionary<(ITypeSymbol ret, ITypeSymbol arg0), PrimitiveReader> PrimitiveReaders { get; }
        public Dictionary<ITypeSymbol, int> SizeOf { get; }

        public WellKnown(Compilation compilation, TypeDiscoverer typeDiscoverer)
        {
            var reflectionPrimitiveFactory = ReflectionPrimitiveReaders.ToDictionary(kv => kv.Key, kv =>
            {
                return new PrimitiveReader((data, args) =>
                {
                    return SyntaxFactory.InvocationExpression(
                            SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                data,
                                SyntaxFactory.IdentifierName(kv.Value)))
                            .AddArgumentListArguments(args);
                });
            });

            PrimitiveReaders = TransformDictionary(reflectionPrimitiveFactory, k => {
                ITypeSymbol t = compilation.GetTypeSymbol(k.Key);
                ITypeSymbol data = compilation.GetTypeSymbol(typeof(Span<byte>));
                return (t, data);
            });

            var streamReaders = TransformDictionary(reflectionPrimitiveFactory, k =>
            {
                ITypeSymbol t = compilation.GetTypeSymbol(k.Key);
                ITypeSymbol data = compilation.GetTypeSymbol(typeof(Stream));
                return (t, data);
            });

            foreach(var sr in streamReaders)
            {
                PrimitiveReaders.Add(sr.Key, sr.Value);
            }

            SizeOf = TransformDictionary<ITypeSymbol, int>(ReflectionSizeOf, k => compilation.GetTypeSymbol(k.Key));

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
                    if(m.IsStatic && 
                        m.GetAttributes().Any(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass, materializerAttr)))
                    {
                        // TODO: check parameter(s)?

                        var lookupType = m.ReturnType;

                        if(m.ReturnType is INamedTypeSymbol named && named.IsGenericType)
                        {
                            lookupType = named.ConstructUnboundGenericType();
                        }

                        PrimitiveReaders[(lookupType, m.Parameters.FirstOrDefault()?.Type)] = (data, args) => SyntaxFactory.InvocationExpression(
                            SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.IdentifierName(s.ToDisplayString()),
                                SyntaxFactory.IdentifierName(m.Name)))
                            .AddArgumentListArguments(SyntaxFactory.Argument(data))
                            .AddArgumentListArguments(args);
                    }
                }
            }
        }

        private Dictionary<TKey, T> TransformDictionary<TKey, T>(Dictionary<Type, T> inDict, Func<KeyValuePair<Type, T>, TKey> keyFunc)
        {
            var dict = new Dictionary<TKey, T>();

            foreach (var kv in inDict)
            {
                var key = keyFunc(kv);

                if (key.Equals(default(TKey)) == false)
                {
                    dict.Add(key, kv.Value);
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
