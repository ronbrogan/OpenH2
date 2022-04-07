using OpenH2.Core.Offsets;
using OpenH2.Core.Tags;
using OpenBlam.Serialization.Layout;

namespace OpenH2.Core.Maps.MCC
{
    [FixedLength(896)]
    public class H2mccV13MapHeader : IH2MapHeader
    {
        [StringValue(0, 4)]
        public string FileHead { get; set; }

        [PrimitiveValue(4)]
        public int Version { get; set; }

        [PrimitiveValue(16)]
        public NormalOffset IndexOffset { get; set; }

        [PrimitiveValue(724)]
        public int RawSecondaryOffset { get; set; }
        public PrimaryOffset SecondaryOffset { get; set; }


        [PrimitiveValue(32)]
        public int FileCount { get; set; }

        [PrimitiveValue(36)]
        public int FileTableOffset { get; set; }

        [PrimitiveValue(40)]
        public int FileTableSize { get; set; }

        [PrimitiveValue(44)]
        public int FilesIndex { get; set; }

        [PrimitiveValue(48)]
        public int InternedStringCount { get; set; }

        [PrimitiveValue(56)]
        public int SizeOfScriptReference { get; set; }

        [PrimitiveValue(60)]
        public int InternedStringIndexOffset { get; set; }

        [PrimitiveValue(52)]
        public int InternedStringsOffset { get; set; }

        [StringValue(176, 32)]
        public string Name { get; set; }

        [StringValue(208, 256)]
        public string ScenarioPath { get; set; }

        [PrimitiveValue(760)]
        public int StoredSignature { get; set; }

        [PrimitiveValue(776)]
        public int CompressionChunkSize { get; set; }

        [PrimitiveValue(780)]
        public int CompressedDataOffset { get; set; }

        [PrimitiveValue(784)]
        public int CompressionIndexOffset { get; set; }

        [PrimitiveValue(788)]
        public int CompressionIndexCount { get; set; }



        [PrimitiveValue(764)]
        public TagRef<SoundMappingTag> LocalSounds { get; set; }

        




        [StringValue(892, 4)]
        public string Footer { get; set; }
    }
}
