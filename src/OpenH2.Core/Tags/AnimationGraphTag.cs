using OpenH2.Core.Maps;
using OpenH2.Core.Tags.Layout;
using OpenH2.Serialization.Layout;

namespace OpenH2.Core.Tags
{
    [TagLabel(TagName.jmad)]
    public class AnimationGraphTag : BaseTag
    {
        public AnimationGraphTag(uint id) : base(id)
        {
        }

        [PrimitiveValue(4)]
        public TagRef<AnimationGraphTag> Unknown { get; set; }


        [ReferenceArray(12)]
        public Obj12[] Obj12s { get; set; }

        [ReferenceArray(44)]
        public PositionTrack[] Tracks { get; set; }

        [ReferenceArray(52)]
        public Obj1556[] Obj1556s { get; set; }

        [ReferenceArray(84)]
        public Obj1656[] Obj1656s { get; set; }





        [FixedLength(32)]
        public class Obj12
        {
            [InternedString(0)]
            public string Description { get; set; }

            [PrimitiveValue(4)]
            public ushort ValueA { get; set; }

            [PrimitiveValue(6)]
            public ushort ValueB { get; set; }

            [PrimitiveValue(8)]
            public ushort ValueC { get; set; }

            [PrimitiveValue(10)]
            public ushort ValueD { get; set; }

            [PrimitiveArray(12, 5)]
            public float[] Params { get; set; }
        }

        [FixedLength(96)]
        public class PositionTrack
        {
            [InternedString(0)]
            public string Description { get; set; }


            [PrimitiveArray(4, 18)]
            public ushort[] Values { get; set; }

            [ReferenceArray(40)]
            public byte[] Data { get; set; }

        }

        [FixedLength(20)]
        public class Obj1556
        {
            [PrimitiveValue(0)]
            public ushort IndexA { get; set; }

            [PrimitiveValue(2)]
            public ushort IndexB { get; set; }

            [PrimitiveArray(4,2)]
            public uint[] Next { get; set; }

            [ReferenceArray(12)]
            public Obj1556Val[] Value { get; set; }
        }


        [FixedLength(8)]
        public class Obj1556Val
        {
            [PrimitiveValue(0)]
            public ushort ValueA { get; set; }

            [PrimitiveValue(2)]
            public ushort ValueB { get; set; }

            [PrimitiveValue(4)]
            public ushort ValueC { get; set; }

            [PrimitiveValue(6)]
            public ushort ValueD { get; set; }
        }

        [FixedLength(8)]
        public class Obj1656
        {
            [PrimitiveValue(0)]
            public ushort ValueA { get; set; }

            [PrimitiveValue(2)]
            public ushort ValueB { get; set; }

            [PrimitiveValue(4)]
            public ushort ValueC { get; set; }

            [PrimitiveValue(6)]
            public ushort ValueD { get; set; }
        }
    }
}
