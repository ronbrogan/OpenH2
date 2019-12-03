using OpenH2.ScenarioExplorer.ViewModels;

namespace OpenH2.ScenarioExplorer.Processors
{
    public interface ITagTreeProcessor
    {
        void PopulateChildren(TagViewModel vm, TagTreeEntryViewModel entry);
    }
}
