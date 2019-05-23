using OpenH2.Core.Tags.Layout;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace OpenH2.Core.Tags.Processors
{
    public static class TagTypeMetadataProvider
    {
        private static Dictionary<Type, TagProperty[]> CachedTagProperties = new Dictionary<Type, TagProperty[]>();
        private static Dictionary<Type, int> CachedFixedLengths = new Dictionary<Type, int>();

        public static TagProperty[] GetProperties(Type tag)
        {
            if (CachedTagProperties.TryGetValue(tag, out var props))
            {
                return props;
            }

            var properties = tag.GetProperties();

            var tagPropertyInfos = new List<TagProperty>(properties.Length);

            foreach (var prop in properties)
            {
                var attr = prop.GetCustomAttribute<TagValueAttribute>();

                if (attr != null)
                {
                    tagPropertyInfos.Add(new TagProperty()
                    {
                        LayoutAttribute = attr,
                        Type = prop.PropertyType,
                        Setter = prop.GetSetMethod(),
                        Getter = prop.GetGetMethod()
                    });
                }
            }

            var result = tagPropertyInfos.ToArray();

            CachedTagProperties.Add(tag, result);

            return result;
        }

        public static int GetFixedLength(Type type)
        {
            if (CachedFixedLengths.TryGetValue(type, out var length))
            {
                return length;
            }

            var attr = type.GetCustomAttribute<FixedLengthAttribute>();

            if (attr == null)
            {
                throw new Exception($"Type {{{type.Name}}} does not have a 'FixedLength' attribute, but should");
            }

            CachedFixedLengths[type] = attr.Length;

            return attr.Length;
        }
    }
}
