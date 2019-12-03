using OpenH2.Core.Representations;
using OpenH2.Core.Tags.Layout;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenH2.Core.Tags
{
    [TagLabel(TagName.jmad)]
    public class AnimationGraph : BaseTag
    {
        public AnimationGraph(uint id) : base(id)
        {
        }

        [PrimitiveValue(4)]
        public TagRef<AnimationGraph> Unknown { get; set; }


        [InternalReferenceValue(12)]
        public Obj172[] Obj172s { get; set; }

        [InternalReferenceValue(44)]
        public Obj588[] Obj588s { get; set; }

        [InternalReferenceValue(52)]
        public Obj1556[] Obj1556s { get; set; }

        [InternalReferenceValue(84)]
        public Obj1656[] Obj1656s { get; set; }





        [FixedLength(32)]
        public class Obj172
        {
            [PrimitiveValue(0)]
            public ushort IndexA { get; set; }

            [PrimitiveValue(2)]
            public ushort IndexB { get; set; }

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

        [FixedLength(100)]
        public class Obj588
        {
            [PrimitiveValue(0)]
            public ushort IndexA { get; set; }

            [PrimitiveValue(2)]
            public ushort IndexB { get; set; }

            [PrimitiveValue(4)]
            public float ValueA { get; set; }

            [PrimitiveArray(6, 16)]
            public ushort[] Values { get; set; }

            [InternalReferenceValue(40)]
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

            [InternalReferenceValue(12)]
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
