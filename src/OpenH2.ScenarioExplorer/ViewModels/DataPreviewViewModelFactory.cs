using OpenH2.Core.Enums;
using OpenH2.Core.Maps;
using OpenH2.Core.Offsets;
using OpenH2.Core.Tags;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenH2.ScenarioExplorer.ViewModels
{
    public static class DataPreviewViewModelFactory
    {
        public static DataPreviewViewModel Create(int offset, TagViewModel tag, H2vMap map)
        {
            var vm = new DataPreviewViewModel(offset, tag);

            if(map.InternedStrings.TryGetValue(vm.Short, out var str))
            {
                vm.InternedString = str;
            }

            if(map.TryGetTag(vm.UInt, out BaseTag t))
            {
                vm.TagName = t.Name;
            }

            var secondaryOffset = map.GetSecondaryOffset(tag.OriginalTag.DataFile, vm.Int);

            vm.FileOffset = tag.OriginalTag.DataFile switch
            {
                DataFile.Local => secondaryOffset.Value.ToString(),
                DataFile.MainMenu => "MM-" + secondaryOffset.Value,
                DataFile.Shared => "MS-" + secondaryOffset.Value,
                DataFile.SinglePlayerShared => "SS-" + secondaryOffset.Value,
            };

            return vm;
        }
    }
}
