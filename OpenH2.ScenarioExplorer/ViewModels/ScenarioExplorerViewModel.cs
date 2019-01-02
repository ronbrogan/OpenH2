using System.Collections.Generic;
using System.Collections.ObjectModel;
using PropertyChanged;
using System.ComponentModel;
using System.Linq;
using Avalonia.Controls;

namespace OpenH2.ScenarioExplorer.ViewModels
{
    [AddINotifyPropertyChangedInterface]
    public class ScenarioExplorerViewModel
    {
        public ScenarioExplorerViewModel()
        {
            
        }

        public ScenarioViewModel LoadedScenario { get; set; }

        private TagTreeEntryViewModel selectedEntry;
        public TagTreeEntryViewModel SelectedEntry
        {
            get => selectedEntry;
            set 
            {
                selectedEntry = value;

                CurrentTag = LoadedScenario.TagLookup[selectedEntry.Id];
            }
        }

        public TagViewModel CurrentTag { get; set; }

        public ObservableCollection<string> RecentFiles { get; set; } = new ObservableCollection<string>
        {
            "D:\\ascension.map"
        };

        public Control[] MenuItems { get; set; }

    }
}
