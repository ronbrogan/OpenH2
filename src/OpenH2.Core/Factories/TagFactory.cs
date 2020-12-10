using OpenBlam.Core.MapLoading;
using OpenBlam.Serialization;
using OpenH2.Core.Extensions;
using OpenH2.Core.Maps;
using OpenH2.Core.Tags;
using OpenH2.Core.Tags.Layout;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace OpenH2.Core.Factories
{
    public static class TagFactory
    {
        public static BaseTag CreateTag(uint id, string name, TagIndexEntry index, IH2Map map, MapStream reader)
        {
            var tagType = GetTypeForTag(index.Tag);

            BaseTag tag;

            if (tagType == null)
            {
                tag = new UnknownTag(id, index.Tag.ToString())
                {
                    Name = name,
                    Length = (uint)index.DataSize,
                    Offset = (uint)index.Offset.Value,
                    InternalSecondaryMagic = map.SecondaryMagic + index.Offset.Value
                };
            }
            else
            {
                BaseTag instance;

                // PERF: check ctor existence ahead of time
                try
                {
                    //var ctor = tagType.GetConstructor(new[] { typeof(uint) });
                    //instance = (BaseTag)ctor.Invoke(new object[] { id });
                    instance = Activator.CreateInstance(tagType, new object[] { id }) as BaseTag;
                }
                catch
                {
                    instance = (BaseTag)FormatterServices.GetUninitializedObject(tagType);
                }

                tag = (BaseTag)BlamSerializer.DeserializeInto(instance,
                    tagType,
                    reader.GetStream(map.OriginFile),
                    index.Offset.Value,
                    map.SecondaryMagic,
                    map);
            }

            tag.Name = name;
            tag.TagIndexEntry = index;
            tag.DataFile = map.OriginFile;
            tag.PopulateExternalData(reader);

            return tag;
        }

        private static Dictionary<TagName, Type> cachedTagTypes = null;

        public static Type GetTypeForTag(TagName tag)
        {
            if(cachedTagTypes == null)
            {
                cachedTagTypes = Assembly.GetAssembly(typeof(BaseTag)).GetTypes()
                    .Where(t => t.IsClass && t.IsSubclassOf(typeof(BaseTag)))
                    .Select(t => new
                    {
                        Label = t.GetCustomAttribute<TagLabelAttribute>()?.Label,
                        Type = t
                    })
                    .Where(e => e != null && Enum.IsDefined(typeof(TagName), e.Label.Value))
                    .ToDictionary(e => e.Label.Value, e => e.Type);
            }

            if(cachedTagTypes.TryGetValue(tag, out var type))
            {
                return type;
            }

            return null;            
        }
    }
}
