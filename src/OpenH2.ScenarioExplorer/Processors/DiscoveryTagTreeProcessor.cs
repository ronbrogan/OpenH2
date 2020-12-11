using OpenBlam.Core.Extensions;
using OpenH2.Core.Extensions;
using OpenH2.Core.Maps.Vista;
using OpenH2.Core.Tags;
using OpenH2.ScenarioExplorer.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenH2.ScenarioExplorer.Processors
{
    public class DiscoveryTagTreeProcessor : ITagTreeProcessor
    {
        private readonly H2vMap scene;

        private (uint, uint) sceneTagIndexRange;

        public DiscoveryTagTreeProcessor(H2vMap scene)
        {
            this.scene = scene;

            uint min = uint.MaxValue;
            uint max = uint.MinValue;

            foreach(var key in scene.TagIndex.Keys)
            {
                min = Math.Min(min, key);
                max = Math.Max(max, key);
            }

            sceneTagIndexRange = (min, max);
        }

        // Read each int32 in the tag data, see if it is a tag ID
        public void PopulateChildren(TagViewModel tag, TagTreeEntryViewModel tagEntry)
        {
            var span = tag.Data.Span;
            var addedChildren = new HashSet<BaseTag>();

            for (var i = 0; i < tag.Data.Length; i += 4)
            {
                var val = span.ReadUInt32At(i);

                if (val < sceneTagIndexRange.Item1 || val > sceneTagIndexRange.Item2)
                    continue;

                if (tag.Id == val)
                    continue;

                if(scene.TryGetTag<BaseTag>(val, out var refTag))
                {
                    if (addedChildren.Contains(refTag) || refTag == null)
                        continue;

                    addedChildren.Add(refTag);
                }
            }

            tagEntry.Children = addedChildren
                .Select(c => new TagTreeEntryViewModel(c))
                .ToArray();
        }
    }
}
