using OpenH2.Core.Offsets;
using OpenH2.Core.Tags;

namespace OpenH2.Core.Maps
{
    public interface IH2MapHeader
    {
        int FileCount { get; set; }
        int FilesIndex { get; set; }
        int FileTableOffset { get; set; }
        int FileTableSize { get; set; }
        NormalOffset IndexOffset { get; set; }
        int InternedStringCount { get; set; }
        int InternedStringIndexOffset { get; set; }
        int InternedStringsOffset { get; set; }
        TagRef<SoundMappingTag> LocalSounds { get; set; }
        string Name { get; set; }
        int RawSecondaryOffset { get; set; }
        string ScenarioPath { get; set; }
        PrimaryOffset SecondaryOffset { get; set; }
        int SizeOfScriptReference { get; set; }
        int StoredSignature { get; set; }
        int Version { get; set; }
    }
}