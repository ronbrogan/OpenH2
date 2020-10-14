using OpenH2.Core.Maps;
using OpenH2.Core.Tags.Layout;
using OpenH2.Serialization.Layout;

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
            public Unknown1[] Unknown1s { get; set; }

            [ReferenceArray(8)]
            public short[] Unknown2s { get; set; }

            [ReferenceArray(16)]
            public ShaderPassReference[] ShaderPassReferences { get; set; }

            [ReferenceArray(24)]
            public Unknown4[] Unknown4s { get; set; }

            [ReferenceArray(32)]
            public Unknown5[] Unknown5s { get; set; }
        }

        [FixedLength(10)]
        public class Unknown1
        {
            [PrimitiveArray(0, 5)]
            public ushort[] Data { get; set; }
        }

        [FixedLength(12)]
        public class ShaderPassReference
        {
            [PrimitiveValue(4)]
            public TagRef<ShaderPassTag> ShaderPass { get; set; }
        }

        [FixedLength(12)]
        public class Unknown4
        {
            [PrimitiveArray(0, 3)]
            public ushort[] Data { get; set; }
        }

        [FixedLength(12)]
        public class Unknown5
        {
            [PrimitiveValue(0)]
            public byte ValueA { get; set; }

            [PrimitiveValue(1)]
            public ushort ValueB { get; set; }
        }
    }
}
