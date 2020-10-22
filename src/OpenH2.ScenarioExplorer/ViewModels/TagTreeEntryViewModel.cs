using OpenH2.Core.Tags;
using PropertyChanged;
using System;

namespace OpenH2.ScenarioExplorer.ViewModels
{
    // Lazily build tagtreeviewmodel children once it is expanded, new entry for every child
    // When an entry is selected, we can lookup the tag and load it 
    [AddINotifyPropertyChangedInterface]
    public class TagTreeEntryViewModel
    {
        public string TagFourCC { get; set; }

        public string TagName { get; set; }

        public uint Id { get; set; }

        public string Description => $"{TagName} ({Id})";

        public TagTreeEntryViewModel[] Children { get; set; } = null;

        public bool NullChildren => Children == null;

        public string Name { get; }

        public TagTreeEntryViewModel(BaseTag tag) : this(tag.Id, tag.TagIndexEntry.Tag.ToString(), tag.Name)
        {
        }

        public TagTreeEntryViewModel(uint id, string tagFourCC, string name)
        {
            this.Id = id;
            this.TagFourCC = tagFourCC;
            this.Name = name;

            this.TagName = tagFourCC + "-" + name;
        }

        public void GenerateCaoCode()
        {
            Console.WriteLine("heyo");
        }

        public void CopyTagName()
        {
            TextCopy.ClipboardService.SetText(this.Name.Replace("\\", "\\\\") + "." + this.TagFourCC);
        }

        public void CopyTagId()
        {
            TextCopy.ClipboardService.SetText(Id.ToString());
        }
    }
}
