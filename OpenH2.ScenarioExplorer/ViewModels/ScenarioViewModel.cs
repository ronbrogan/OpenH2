using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using OpenH2.Core.Extensions;
using OpenH2.Core.Representations;
using OpenH2.Core.Tags;
using PropertyChanged;

namespace OpenH2.ScenarioExplorer.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class ScenarioViewModel
    {
        public List<TagViewModel> Tags { get; set; }

        public Dictionary<uint, TagViewModel> TagLookup = new Dictionary<uint, TagViewModel>();

        public HashSet<uint> PostprocessedTags = new HashSet<uint>();

        public TagTreeEntryViewModel[] TreeRoots { get; set; }

        public ScenarioViewModel() { }

        public ScenarioViewModel(Scene scene)
        {
            var sceneData = scene.RawData.Span;

            this.Tags = new List<TagViewModel>(scene.TagIndex.Count);

            foreach (var tagEntry in scene.TagIndex)
            {
                var vm = new TagViewModel(tagEntry.ID, tagEntry.Tag)
                {
                    InternalOffsetStart = tagEntry.Offset.OriginalValue,
                    InternalOffsetEnd = tagEntry.Offset.OriginalValue + tagEntry.MetaSize,
                    Data = sceneData.Slice(tagEntry.Offset.Value, tagEntry.MetaSize).ToArray(),
                };

                BaseTag tag = null;
                scene.Tags.TryGetValue(tagEntry.ID, out tag);

                vm.OriginalTag = tag;

                Tags.Add(vm);
            }

            TagLookup = Tags.ToDictionary(t => t.Id);

            var scenarioEntry = new TagTreeEntryViewModel()
            {
                Id = (uint)scene.IndexHeader.ScenarioID,
                TagName = "scnr"
            };

            BuildTree(scenarioEntry, new HashSet<uint>());

            TreeRoots = new[] { scenarioEntry };
        }

        private void BuildTree(TagTreeEntryViewModel entry, HashSet<uint> ancestors)
        {
            ancestors.Add(entry.Id);
            PopulateChildren(entry);

            foreach(var child in entry.Children)
            {
                if(ancestors.Contains(child.Id))
                {
                    Console.WriteLine("Cycle in graph");
                    continue;
                }

                var newAnc = new HashSet<uint>(ancestors);
                
                BuildTree(child, newAnc);
            }
        }

        // Read each int32 in the tag data, see if it is a tag ID
        private void PopulateChildren(TagTreeEntryViewModel tagEntry)
        {
            var tag = this.TagLookup[tagEntry.Id];

            var span = tag.Data.Span;
            var addedChildren = new HashSet<TagViewModel>();

            for (var i = 0; i < tag.Data.Length; i += 4)
            {
                var val = span.ReadUInt32At(i);

                if (TagLookup.TryGetValue(val, out var referenceTag))
                {
                    if (addedChildren.Contains(referenceTag))
                        continue;

                    if (tag == referenceTag)
                        continue;

                    addedChildren.Add(referenceTag);
                }
            }

            tagEntry.Children = addedChildren
                .Select(c => new TagTreeEntryViewModel()
                {
                    Id = c.Id,
                    TagName = c.Name
                })
                .ToArray();
        }

        public TagViewModel GetTagViewModel(uint tagId)
        {
            var tag = this.TagLookup[tagId];

            if(this.PostprocessedTags.Contains(tagId) == false)
            {
                tag.GeneratePointsOfInterest();
                this.PostprocessedTags.Add(tagId);
            }

            return tag;
        }


    }
}
