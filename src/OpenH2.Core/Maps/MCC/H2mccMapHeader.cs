using OpenH2.Core.Offsets;
using OpenH2.Core.Tags;
using OpenBlam.Serialization.Layout;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenH2.Core.Maps.MCC
{
    [FixedLength(4096)]
    public class H2mccMapHeader : IH2MapHeader
    {
        [StringValue(0, 4)]
        public string FileHead { get; set; }

        [PrimitiveValue(4)]
        public int Version { get; set; }

        [PrimitiveValue(12)]
        public NormalOffset IndexOffset { get; set; }

        [PrimitiveValue(16)]
        public int RawSecondaryOffset { get; set; }
        public PrimaryOffset SecondaryOffset { get; set; }

        [PrimitiveValue(356)]
        public int OffsetToUnknownSection { get; set; }

        [PrimitiveValue(360)]
        public int InternedStringCount { get; set; }

        [PrimitiveValue(364)]
        public int SizeOfScriptReference { get; set; }

        [PrimitiveValue(368)]
        public int InternedStringIndexOffset { get; set; }

        [PrimitiveValue(372)]
        public int InternedStringsOffset { get; set; }

        [StringValue(448, 32)]
        public string Name { get; set; }

        [StringValue(484, 256)]
        public string ScenarioPath { get; set; }

        [PrimitiveValue(744)]
        public int FileCount { get; set; }

        [PrimitiveValue(748)]
        public int FileTableOffset { get; set; }

        [PrimitiveValue(752)]
        public int FileTableSize { get; set; }

        [PrimitiveValue(756)]
        public int FilesIndex { get; set; }

        [PrimitiveValue(768)]
        public TagRef<SoundMappingTag> LocalSounds { get; set; }

        [PrimitiveValue(780)]
        public int StoredSignature { get; set; }

        [StringValue(2044, 4)]
        public string Footer { get; set; }
    }
}
