using OpenH2.Core.Offsets;
using OpenH2.Core.Tags;
using OpenH2.Serialization.Layout;

namespace OpenH2.Core.Maps
{
    [FixedLength(2048)]
    public class H2vMapHeader : IH2MapHeader
    {
        [StringValue(0, 4)]
        public string FileHead { get; set; }

        [PrimitiveValue(4)]
        public int Version { get; set; }

        [PrimitiveValue(8)]
        public int TotalBytes { get; set; }

        [PrimitiveValue(16)]
        public NormalOffset IndexOffset { get; set; }

        [PrimitiveValue(20)]
        public int RawSecondaryOffset { get; set; }
        public PrimaryOffset SecondaryOffset { get; set; }

        [StringValue(32, 32)]
        public string MapOrigin { get; set; }

        [StringValue(300, 32)]
        public string Build { get; set; }

        [PrimitiveValue(364)]
        public int OffsetToUnknownSection { get; set; }

        [PrimitiveValue(368)]
        public int InternedStringCount { get; set; }

        [PrimitiveValue(372)]
        public int SizeOfScriptReference { get; set; }

        [PrimitiveValue(376)]
        public int InternedStringIndexOffset { get; set; }

        [PrimitiveValue(380)]
        public int InternedStringsOffset { get; set; }

        [StringValue(420, 32)]
        public string Name { get; set; }

        [StringValue(456, 256)]
        public string ScenarioPath { get; set; }

        [PrimitiveValue(716)]
        public int FileCount { get; set; }

        [PrimitiveValue(720)]
        public int FileTableOffset { get; set; }

        [PrimitiveValue(724)]
        public int FileTableSize { get; set; }

        [PrimitiveValue(728)]
        public int FilesIndex { get; set; }

        [PrimitiveValue(740)]
        public TagRef<SoundMappingTag> LocalSounds { get; set; }

        [PrimitiveValue(752)]
        public int StoredSignature { get; set; }

        [StringValue(2044, 4)]
        public string Footer { get; set; }
    }
}