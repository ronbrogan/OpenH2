using OpenH2.Core.Maps;
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

            return vm;
        }
    }
}
