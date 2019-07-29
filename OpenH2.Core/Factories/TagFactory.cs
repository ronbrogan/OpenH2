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

        public static BaseTag CreateTag(uint id, string name, TagIndexEntry index, TrackingChunk chunk, H2vReader reader)
        {
            var tagType = GetTypeForTag(index.Tag);

            if (tagType == null)
                return null;

            var tagCreator = generator.GetTagCreator(tagType);

            var tag = tagCreator(id, name, chunk.Span, index.Offset.OriginalValue) as BaseTag;

            tag.PopulateExternalData(reader);

            return tag as BaseTag;
        }

        private static Dictionary<string, Type> cachedTagTypes = null;

        private static Type GetTypeForTag(string tag)
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
                    .Where(e => string.IsNullOrWhiteSpace(e.Label) == false)
                    .ToDictionary(e => e.Label, e => e.Type);
            }

            if(cachedTagTypes.TryGetValue(tag, out var type))
            {
                return type;
            }

            return null;            
        }
    }
}
