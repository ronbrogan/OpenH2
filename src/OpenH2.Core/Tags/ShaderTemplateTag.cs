using OpenH2.Core.Maps;
using OpenH2.Core.Tags.Layout;
using OpenH2.Serialization.Layout;
using System;

namespace OpenH2.Core.Tags
{
    [TagLabel(TagName.stem)]
    public class ShaderTemplateTag : BaseTag
    {
        public override string Name { get; set; }

        public ShaderTemplateTag(uint id) : base(id)
        {
        }

        [ReferenceArray(88)]
        public ShaderInfo[] ShaderInfos { get; set; }

        [FixedLength(40)]
        public class ShaderInfo
        {
            [ReferenceArray(0)]
            public Obj0[] Obj0s { get; set; }

            [ReferenceArray(8)]
            public Obj8[] Unknown2s { get; set; }

            [ReferenceArray(16)]
            public ShaderLod[] Lods { get; set; }

            [ReferenceArray(24)]
            public Obj24[] Obj24s { get; set; }


            // Default values?
            [ReferenceArray(32)]
            public Obj32[] Obj32s { get; set; }

            [FixedLength(10)]
            public class Obj0
            {
                [PrimitiveArray(0, 5)]
                public ushort[] Data { get; set; }
            }

            [FixedLength(2)]
            public class Obj8
            {
                [PrimitiveValue(0)]
                public byte ShaderPassIndex { get; set; }

                [PrimitiveValue(1)]
                public byte B { get; set; }
            }

            [FixedLength(6)]
            public class Obj24
            {
                [PrimitiveValue(0)]
                public byte Obj32IndexA { get; set; }

                [PrimitiveValue(1)]
                public byte OtherA { get; set; }

                [PrimitiveValue(2)]
                public byte Obj32IndexB { get; set; }

                [PrimitiveValue(3)]
                public byte OtherB { get; set; }

                [PrimitiveValue(4)]
                public byte Obj32IndexC { get; set; }

                [PrimitiveValue(5)]
                public byte OtherC { get; set; }
            }

            [FixedLength(4)]
            public class Obj32
            {
                [PrimitiveValue(0)]
                public byte A { get; set; }

                public string Aflags => Convert.ToString(A, 2).PadLeft(8, '0');

                [PrimitiveValue(1)]
                public byte B { get; set; }

                [PrimitiveValue(2)]
                public byte C { get; set; }

                [PrimitiveValue(3)]
                public byte D { get; set; }
            }
        }

        [FixedLength(12)]
        public class ShaderLod
        {
            [PrimitiveValue(4)]
            public TagRef<ShaderPassTag> ShaderPass { get; set; }

            [PrimitiveArray(0, 2)]
            public ushort[] Data { get; set; }
        }
    }
}
