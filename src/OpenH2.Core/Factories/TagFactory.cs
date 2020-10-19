using OpenH2.Core.Parsing;
using OpenH2.Core.Maps;
using OpenH2.Core.Tags;
using OpenH2.Core.Tags.Layout;
using OpenH2.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.IO;

namespace OpenH2.Core.Factories
{
    public static class TagFactory
    {
        public static BaseTag CreateTag(uint id, string name, TagIndexEntry index, H2BaseMap map, H2MapReader reader)
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
                var mapData = reader.MapReader;

                // Preload tag data for faster reads
                mapData.Preload(index.Offset.Value, index.DataSize);

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
                    reader.MapReader.Data,
                    index.Offset.Value,
                    map.SecondaryMagic,
                    map);
            }

            tag.Name = name;
            tag.TagIndexEntry = index;
            tag.DataFile = reader.GetPrimaryDataFile();
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
