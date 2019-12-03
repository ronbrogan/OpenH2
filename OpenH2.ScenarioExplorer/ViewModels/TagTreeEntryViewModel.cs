using PropertyChanged;

namespace OpenH2.ScenarioExplorer.ViewModels
{
    // Lazily build tagtreeviewmodel children once it is expanded, new entry for every child
    // When an entry is selected, we can lookup the tag and load it 
    [AddINotifyPropertyChangedInterface]
    public class TagTreeEntryViewModel
    {
        public string TagName { get; set; }

        public uint Id { get; set; }

        public string Description => $"{TagName} ({Id})";

        public TagTreeEntryViewModel[] Children { get; set; } = null;

        public bool NullChildren => Children == null;
    }
}
