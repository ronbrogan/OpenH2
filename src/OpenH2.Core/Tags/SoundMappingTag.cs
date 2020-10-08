using OpenH2.Core.Offsets;
using OpenH2.Core.Maps;
using OpenH2.Core.Tags.Layout;
using OpenH2.Serialization.Layout;

namespace OpenH2.Core.Tags
{
    [TagLabel(TagName.ugh)]
    public class SoundMappingTag : BaseTag
    {
        public SoundMappingTag(uint id) : base(id)
        {
        }

        [ReferenceArray(0)]  public Obj0[] Obj0s { get; set; }
        [ReferenceArray(8)]  public Obj8[] Obj8s { get; set; }
        [ReferenceArray(16)] public NameInfo[] NameInfos { get; set; }
        [ReferenceArray(24)] public Obj24[] Obj24s { get; set; }
        [ReferenceArray(32)] public SoundEntry[] SoundEntries { get; set; }
        [ReferenceArray(40)] public NamedSoundSample[] NamedSoundClips { get; set; }
        [ReferenceArray(56)] public byte[] ZeroPadding { get; set; }
        [ReferenceArray(64)] public SoundDataChunk[] SoundDataChunks { get; set; }
        [ReferenceArray(72)] public Obj72[] Obj72s { get; set; }
        [ReferenceArray(80)] public DataInfo[] DataInfos { get; set; }

        [FixedLength(56)]
        public class Obj0
        {
            [PrimitiveValue(0)]
            public float Unknown1 { get; set; }

            [PrimitiveValue(4)]
            public float Unknown2 { get; set; }

            [PrimitiveValue(16)]
            public float Unknown3 { get; set; }

            [PrimitiveValue(20)]
            public float Unknown4 { get; set; }

            [PrimitiveValue(24)]
            public ushort IndexA { get; set; }

            [PrimitiveValue(26)]
            public ushort IndexB { get; set; }

            [PrimitiveValue(28)]
            public float ParamA { get; set; }

            [PrimitiveValue(32)]
            public float ParamB { get; set; }
        }

        [FixedLength(20)]
        public class Obj8
        {
            [PrimitiveValue(0)]
            public float Unknown1 { get; set; }

            [PrimitiveValue(4)]
            public float Unknown2 { get; set; }

            [PrimitiveValue(8)]
            public ushort IndexA { get; set; }

            [PrimitiveValue(10)]
            public ushort IndexB { get; set; }

            [PrimitiveValue(12)]
            public float Unknown3 { get; set; }

            [PrimitiveValue(16)]
            public float Unknown4 { get; set; }
        }

        [FixedLength(4)]
        public class NameInfo
        {
            [InternedString(0)]
            public string Name { get; set; }
        }

        [FixedLength(10)]
        public class Obj24
        {
            [PrimitiveValue(0)]
            public ushort IndexA { get; set; }

            [PrimitiveValue(2)]
            public ushort IndexB { get; set; }

            [PrimitiveValue(4)]
            public ushort IndexC { get; set; }

            [PrimitiveValue(6)]
            public ushort IndexD { get; set; }

            [PrimitiveValue(8)]
            public ushort IndexE { get; set; }
        }

        [FixedLength(12)]
        public class SoundEntry
        {
            // Nearly always 431?
            [PrimitiveValue(0)]
            public ushort IndexA { get; set; }

            // Nearly always 0?
            [PrimitiveValue(2)]
            public ushort IndexB { get; set; }

            // Usually 0, sometimes 65535
            [PrimitiveValue(4)]
            public ushort IndexC { get; set; }

            // NOT Obj64 index
            [PrimitiveValue(6)]
            public ushort Flags { get; set; }

            [PrimitiveValue(8)]
            public ushort NamedSoundClipIndex { get; set; }

            [PrimitiveValue(10)]
            public ushort NamedSoundClipCount { get; set; }
        }

        [FixedLength(16)]
        public class NamedSoundSample
        {
            [PrimitiveValue(0)]
            public ushort NameIndex { get; set; }

            [PrimitiveValue(2)]
            public ushort IndexB { get; set; }

            [PrimitiveValue(4)]
            public ushort Flags { get; set; }

            [PrimitiveValue(6)]
            public ushort IndexD { get; set; }

            [PrimitiveValue(8)]
            public ushort IndexE { get; set; }

            [PrimitiveValue(10)]
            public ushort IndexF { get; set; }

            [PrimitiveValue(12)]
            public ushort SoundDataChunkIndex { get; set; }

            [PrimitiveValue(14)]
            public ushort SoundDataChunkCount { get; set; }
        }

        [FixedLength(12)]
        public class SoundDataChunk
        {
            [PrimitiveValue(0)]
            public NormalOffset Offset { get; set; }

            [PrimitiveValue(4)]
            public uint Length { get; set; }

            [PrimitiveValue(8)]
            public uint MaxValue { get; set; }
        }

        [FixedLength(28)]
        public class Obj72
        {
            [ReferenceArray(0)] public Obj72_0[] Obj0s { get; set; }
            [ReferenceArray(8)] public Obj72_8[] Obj8s { get; set; }

            [FixedLength(16)]
            public class Obj72_0
            {
                [PrimitiveValue(0)]
                public ushort IndexA { get; set; }

                [PrimitiveValue(2)]
                public ushort IndexB { get; set; }

                [PrimitiveValue(4)]
                public ushort IndexC { get; set; }

                [PrimitiveValue(6)]
                public ushort IndexD { get; set; }

                [PrimitiveValue(8)]
                public ushort IndexE { get; set; }

                [PrimitiveValue(10)]
                public ushort IndexF { get; set; }

                [PrimitiveValue(12)]
                public ushort IndexG { get; set; }

                [PrimitiveValue(14)]
                public ushort IndexH { get; set; }
            }

            [FixedLength(4)]
            public class Obj72_8
            {
                [PrimitiveValue(0)]
                public ushort IndexA { get; set; }

                [PrimitiveValue(2)]
                public ushort IndexB { get; set; }
            }
        }

        [FixedLength(44)]
        public class DataInfo
        {
            // Lots of stuff here

            [PrimitiveValue(8)]
            public uint BlkhOffset { get; set; }

            [PrimitiveValue(12)]
            public uint BlkhLength { get; set; }

            [PrimitiveValue(16)]
            public uint DataOffsetMaybe { get; set; }

            [PrimitiveValue(20)]
            public uint DataLengthMaybe { get; set; }

            [ReferenceArray(24)]
            public SubResourceInfo[] SubResources { get; set; }

            [PrimitiveValue(32)]
            public TagRef UghRef { get; set; }

            [FixedLength(16)]
            public class SubResourceInfo
            {
                [PrimitiveValue(0)]
                public ushort IndexA { get; set; }

                [PrimitiveValue(2)]
                public ushort IndexB { get; set; }

                [PrimitiveValue(4)]
                public ushort IndexC { get; set; }

                [PrimitiveValue(6)]
                public ushort IndexD { get; set; }

                [PrimitiveValue(8)]
                public uint DataLength { get; set; }
            }
        }
    }
}
