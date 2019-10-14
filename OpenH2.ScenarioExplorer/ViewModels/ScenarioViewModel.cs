using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using OpenH2.Core.Extensions;
using OpenH2.Core.Representations;
using OpenH2.Core.Tags;
using OpenH2.Core.Tags.Layout;
using PropertyChanged;

namespace OpenH2.ScenarioExplorer.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class ScenarioViewModel
    {
        public List<TagViewModel> Tags { get; set; }

        public Dictionary<uint, TagViewModel> TagLookup = new Dictionary<uint, TagViewModel>();

        public HashSet<uint> PostprocessedTags = new HashSet<uint>();
        private readonly H2vMap scene;

        public TagTreeEntryViewModel[] TreeRoots { get; set; }

        public ScenarioViewModel() { }

        public ScenarioViewModel(H2vMap scene, Memory<byte> sceneData)
        {
            var scenarioEntry = new TagTreeEntryViewModel()
            {
                Id = (uint)scene.IndexHeader.ScenarioID,
                TagName = "scnr"
            };

            var discoveryMode = false;
            if(discoveryMode)
            {
                PreProcessTags(scene, sceneData);
                BuildDiscoveryTree(scenarioEntry, new HashSet<uint>());
            }
            else
            {
                BuildExplorationTree(scene, scenarioEntry);
            }

            TreeRoots = new[] { scenarioEntry };
            this.scene = scene;
        }

        private void BuildExplorationTree(H2vMap scene, TagTreeEntryViewModel entry)
        {
            if(scene.TryGetTag<BaseTag>(entry.Id, out var tag) == false)
            {
                return;
            }

            var childRefs = GetChildReferences(tag);

            var childrenVms = new List<TagTreeEntryViewModel>();

            foreach(var child in childRefs)
            {
                if (scene.TryGetTag<BaseTag>(child.Id, out var childTag) == false)
                    continue;

                var tagLabel = childTag.GetType().GetCustomAttribute<TagLabelAttribute>().Label;
                var tagName = tagLabel + (childTag.Name != null ? " - " + childTag.Name : string.Empty);

                childrenVms.Add(new TagTreeEntryViewModel()
                {
                    Id = child.Id,
                    TagName = tagName
                });
            }

            entry.Children = childrenVms.ToArray();

            foreach (var child in entry.Children)
            {
                BuildExplorationTree(scene, child);
            }
        }

        private ITagRef[] GetChildReferences(BaseTag tag)
        {
            var refProps = new List<(object, PropertyInfo)>();
            CollectRecursiveTagRefs(tag, refProps);
            
            var refs = new ITagRef[refProps.Count];

            for (var i = 0; i < refProps.Count; i++)
            {
                var (obj, prop) = refProps[i];

                var val = prop.GetValue(obj);

                refs[i] = (ITagRef)val;
            }

            return refs;

            void CollectRecursiveTagRefs(object current, List<(object, PropertyInfo)> props)
            {
                if (current == null)
                    return;

                var currentProps = current.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

                var tagRefProps = currentProps.Where(t => (t.PropertyType.IsGenericType && t.PropertyType.GetGenericTypeDefinition() == typeof(TagRef<>)) 
                        || t.PropertyType == typeof(TagRef))
                    .ToList();

                foreach(var prop in tagRefProps)
                {
                    props.Add((current, prop));
                }

                var nestedArrays = currentProps.Where(t => (t.PropertyType.IsArray && t.PropertyType.GetElementType().IsClass));

                foreach (var nested in nestedArrays)
                {
                    var arr = (object[])nested.GetValue(current);

                    if (arr == null)
                        continue;

                    foreach(var arrItem in arr)
                    {
                        CollectRecursiveTagRefs(arrItem, props);
                    }
                }

                var nestedItems = currentProps.Where(t => t.PropertyType.IsClass);

                foreach (var nested in nestedItems)
                {
                    CollectRecursiveTagRefs(nested.GetValue(current), props);
                }
            }
        }

        private void PreProcessTags(H2vMap scene, Memory<byte> sceneData)
        {
            this.Tags = new List<TagViewModel>(scene.TagIndex.Length);

            foreach (var tagEntry in scene.TagIndex)
            {
                scene.TryGetTag<BaseTag>(tagEntry.ID, out var tag);

                var vm = new TagViewModel(tagEntry.ID, tagEntry.Tag, tag?.Name)
                {
                    InternalOffsetStart = tagEntry.Offset.OriginalValue,
                    InternalOffsetEnd = tagEntry.Offset.OriginalValue + tagEntry.DataSize,
                    Data = sceneData.Slice(tagEntry.Offset.Value, tagEntry.DataSize).ToArray(),
                    RawOffset = tagEntry.Offset.Value
                };

                vm.OriginalTag = tag;

                Tags.Add(vm);
            }

            TagLookup = Tags.ToDictionary(t => t.Id);
        }

        private void BuildDiscoveryTree(TagTreeEntryViewModel entry, HashSet<uint> ancestors)
        {
            ancestors.Add(entry.Id);
            PopulateDiscoveryChildren(entry);

            foreach(var child in entry.Children)
            {
                if(ancestors.Contains(child.Id))
                {
                    Console.WriteLine("Cycle in graph");
                    continue;
                }

                var newAnc = new HashSet<uint>(ancestors);
                
                BuildDiscoveryTree(child, newAnc);
            }
        }

        // Read each int32 in the tag data, see if it is a tag ID
        private void PopulateDiscoveryChildren(TagTreeEntryViewModel tagEntry)
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
            var discoveryMode = false;
            if (discoveryMode)
            {
                return GetDiscoveryTagViewModel(tagId);
            }
            else
            {
                return GetExplorationTagViewModel(tagId);
            }
        }

        private TagViewModel GetDiscoveryTagViewModel(uint tagId)
        {
            var tag = this.TagLookup[tagId];

            if (this.PostprocessedTags.Contains(tagId) == false)
            {
                tag.GeneratePointsOfInterest();
                this.PostprocessedTags.Add(tagId);
            }

            return tag;
        }

        private TagViewModel GetExplorationTagViewModel(uint tagId)
        {
            scene.TryGetTag<BaseTag>(tagId, out var tag);
            var tagLabel = tag.GetType().GetCustomAttribute<TagLabelAttribute>().Label;

            var vm = new TagViewModel(tagId, tagLabel, tag?.Name)
            {
#if DEBUG
                InternalOffsetStart = tag.InternalSecondaryMagic,
                InternalOffsetEnd = tag.InternalSecondaryMagic + tag.RawData.Length,
#endif
                Data = tag.RawData,
                RawOffset = (int)tag.Offset
            };

            vm.OriginalTag = tag;

            return vm;
        }

    }
}
