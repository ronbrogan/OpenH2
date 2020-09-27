using OpenH2.Core.Tags.Layout;
using OpenH2.Serialization.Layout;

namespace OpenH2.Core.Tags
{
    [TagLabel(TagName.snd)] // snd!
    public class SoundTag : BaseTag
    {
        public override string Name { get; set; }
        public SoundTag(uint id) : base(id)
        {
        }

        [PrimitiveValue(0)]
        public byte Option1 { get; set; }

        [PrimitiveValue(1)]
        public byte Option2 { get; set; }

        [PrimitiveValue(2)]
        public byte Option3 { get; set; }

        [PrimitiveValue(3)]
        public byte Option4 { get; set; }

        [PrimitiveValue(4)]
        public ushort Flags { get; set; }

        [PrimitiveValue(6)]
        public ushort Unknown { get; set; }

        [PrimitiveValue(8)]
        public ushort SoundIndex { get; set; }

        [PrimitiveValue(10)]
        public ushort LoopLength { get; set; }

        [PrimitiveValue(12)]
        public ushort UsuallyMaxValue { get; set; }

        [PrimitiveValue(14)]
        public ushort DialogId { get; set; }

        [PrimitiveValue(16)]
        public ushort Duration { get; set; }

        [PrimitiveValue(18)]
        public ushort UsuallyZero { get; set; }
    }
}
