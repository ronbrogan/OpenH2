using OpenH2.Core.Representations;
using OpenH2.Core.Tags.Layout;
using OpenH2.Serialization.Layout;
using System.Numerics;

namespace OpenH2.Core.Tags
{
    [TagLabel(TagName.lsnd)]
    public class LoopingSoundTag : BaseTag
    {
        public override string Name { get; set; }
        public LoopingSoundTag(uint id) : base(id)
        {
        }

        [PrimitiveValue(12)]
        public float Value1 { get; set; }

        [PrimitiveValue(16)]
        public float Value2 { get; set; }

        [PrimitiveValue(20)]
        public TagRef CdmgTag { get; set; }

        [ReferenceArray(36)]
        public SoundReference[] SoundReferences { get; set; }

        [FixedLength(48)]
        public class SoundReference
        {
            [PrimitiveValue(0)]
            public uint Value1 { get; set; }

            [PrimitiveValue(8)]
            public TagRef<SoundTag> Sound { get; set; }

            [PrimitiveValue(12)]
            public float Value2 { get; set; }

            [PrimitiveValue(16)]
            public float Value3 { get; set; }

            [PrimitiveValue(20)]
            public uint Value4 { get; set; }

            [PrimitiveArray(24, 4)]
            // Maybe indicating sound coverage of a circle? to simulate directional sound?
            public float[] PiValues { get; set; }
        }
    }
}
