using OpenH2.Core.Representations;
using OpenH2.Core.Tags;
using OpenH2.ScenarioExplorer.Processors;
using PropertyChanged;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reactive;

namespace OpenH2.ScenarioExplorer.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class ScenarioViewModel
    {
        public List<TagViewModel> Tags { get; set; }

        public Dictionary<uint, TagViewModel> TagLookup = new Dictionary<uint, TagViewModel>();

        private readonly H2vMap scene;
        private readonly Memory<byte> sceneData;
        private readonly bool discoveryMode;

        private ITagTreeProcessor treeProcessor;

        public TagTreeEntryViewModel[] TreeRoots { get; set; }

        public ReactiveCommand<Unit, Unit> GenerateCaoCode { get; set; }

        public ScenarioViewModel() { }

        public ScenarioViewModel(H2vMap scene, Memory<byte> sceneData, bool discoveryMode = true)
        {
            this.scene = scene;
            this.sceneData = sceneData;
            this.discoveryMode = discoveryMode;

            this.GenerateCaoCode = ReactiveCommand.Create(GenerateCaoCodeM);

            var scenarioVm = GetTagViewModel(scene.IndexHeader.Scenario);
            var scenarioEntry = new TagTreeEntryViewModel()
            {
                Id = scene.IndexHeader.Scenario,
                TagName = "scnr - " + scene.TagNames[scene.IndexHeader.Scenario]
            };


            if (discoveryMode)
            {
                treeProcessor = new DiscoveryTagTreeProcessor(scene);
            }
            else
            {
                treeProcessor = new ExplorationTagTreeProcessor(scene);
            }

            treeProcessor.PopulateChildren(scenarioVm, scenarioEntry);

            this.TreeRoots = new[] { scenarioEntry };
        }

        internal void PopulateTreeChildren(TagTreeEntryViewModel selectedEntry)
        {
            var vm = GetTagViewModel(selectedEntry.Id);
            treeProcessor.PopulateChildren(vm, selectedEntry);
        }

        public void GenerateCaoCodeM()
        {
            Console.WriteLine("heyo");
        }

        public TagViewModel GetTagViewModel(uint tagId)
        {
            if(TagLookup.TryGetValue(tagId, out var existingVm))
            {
                return existingVm;
            }

            if(scene.TryGetTag<BaseTag>(tagId, out var tag) == false)
            {
                return null;
            }

            var indexEntry = tag.TagIndexEntry;

            var vm = new TagViewModel(indexEntry.ID, indexEntry.Tag.ToString(), tag?.Name ?? indexEntry.Tag.ToString())
            {
                // TODO: need to add scene secondary magic to InternalOffsets?
                InternalOffsetStart = indexEntry.Offset.OriginalValue,
                InternalOffsetEnd = indexEntry.Offset.OriginalValue + indexEntry.DataSize,
                RawOffset = indexEntry.Offset.Value
            };

            if (vm.Data.IsEmpty && tag.DataFile == Core.Enums.DataFile.Local)
            {
                vm.Data = sceneData.Slice(indexEntry.Offset.Value, indexEntry.DataSize);
            }

            vm.OriginalTag = tag;
            vm.GeneratePointsOfInterest();

            TagLookup.Add(tagId, vm);

            return vm;
        }
    }
}
