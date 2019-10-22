using OpenH2.Core.Parsing;
using OpenH2.Core.Representations;
using OpenH2.Core.Tags;
using OpenH2.Core.Tags.Layout;
using OpenH2.Core.Tags.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OpenH2.Core.Factories
{
    public static class TagFactory
    {
        private static TagCreatorGenerator generator = new TagCreatorGenerator();

        public static BaseTag CreateTag(uint id, string name, TagIndexEntry index, int secondaryMagic, H2vReader reader)
        {
            var tagType = GetTypeForTag(index.Tag);

            if (tagType == null)
                return null;

            var tagCreator = generator.GetTagCreator(tagType);

            var mapData = reader.MapReader;

            // Preload tag data for faster reads
            mapData.Preload(index.Offset.Value, index.DataSize);
            
            var tag = tagCreator(id, name, mapData, secondaryMagic, index.Offset.Value, index.DataSize) as BaseTag;

            tag.TagIndexEntry = index;
            tag.DataFile = reader.GetPrimaryDataFile();
            tag.PopulateExternalData(reader);

            return tag;
        }

        private static Dictionary<TagName, Type> cachedTagTypes = null;

        private static Type GetTypeForTag(TagName tag)
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
