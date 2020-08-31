using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenH2.Serialization.Layout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OpenH2.Serialization
{
    internal class LayoutInfo
    {
        private static Type[] memberAttributes = new[]
        {
            typeof(PrimitiveValueAttribute),
            typeof(PrimitiveArrayAttribute),
            typeof(ReferenceArrayAttribute),
            typeof(StringValueAttribute),
            typeof(InternedStringAttribute),
        };

        public int? Size { get; }
        public MemberInfo[] MemberInfos { get; }

        public static LayoutInfo Create(Compilation compilation, INamedTypeSymbol type)
        {
            var baseAttr = compilation.GetTypeSymbol<SerializableMemberAttribute>();

            var memberAttributeMap = memberAttributes.ToDictionary(a => compilation.GetTypeSymbol(a), a => a);

            var memberInfo = GetSerializableMembers(type, baseAttr, memberAttributeMap);

            return new LayoutInfo(0, memberInfo);
        }

        private static MemberInfo[] GetSerializableMembers(INamedTypeSymbol type,
            INamedTypeSymbol baseMemberAttr,
            Dictionary<INamedTypeSymbol, Type> memberAttributeMap)
        {
            var members = new List<MemberInfo>();

            var allMembers = type.GetMembers();

            foreach (var member in allMembers)
            {
                var layoutAttrs = member.GetAttributes()
                        .Where(a => SymbolEqualityComparer.Default.Equals(a.AttributeClass.BaseType, baseMemberAttr));

                if (layoutAttrs.Any() == false)
                {
                    continue;
                }

                if (member is IPropertySymbol property)
                {
                    var chosen = layoutAttrs.First();

                    if (memberAttributeMap.TryGetValue(chosen.AttributeClass, out var attrType)
                        && attributeFactories.TryGetValue(attrType, out var factory))
                    {
                        members.Add(new MemberInfo()
                        {
                            LayoutAttribute = factory(chosen),
                            Type = property.Type,
                            PropertySymbol = property
                        });
                    }
                }
            }

            return members.ToArray();
        }

        private static Dictionary<Type, Func<AttributeData, SerializableMemberAttribute>> attributeFactories = new Dictionary<Type, Func<AttributeData, SerializableMemberAttribute>>
        {
            {typeof(PrimitiveValueAttribute), a => new PrimitiveValueAttribute((int)a.ConstructorArguments[0].Value) },
            {typeof(PrimitiveArrayAttribute), a => new PrimitiveArrayAttribute((int)a.ConstructorArguments[0].Value, (int)a.ConstructorArguments[1].Value) },
            {typeof(ReferenceArrayAttribute), a => new ReferenceArrayAttribute((int)a.ConstructorArguments[0].Value) },
            {typeof(StringValueAttribute), a => new StringValueAttribute((int)a.ConstructorArguments[0].Value, (int)a.ConstructorArguments[1].Value) },
            {typeof(InternedStringAttribute), a => new InternedStringAttribute((int)a.ConstructorArguments[0].Value) },
        };

    private LayoutInfo(int? size, MemberInfo[] infos)
        {
            this.Size = size;
            this.MemberInfos = infos;
        }

        public class MemberInfo
        {
            public SerializableMemberAttribute LayoutAttribute { get; set; }

            public ITypeSymbol Type { get; set; }

            public IPropertySymbol PropertySymbol { get; set; }
        }

    }
}
