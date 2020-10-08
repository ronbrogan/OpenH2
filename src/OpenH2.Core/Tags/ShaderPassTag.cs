using OpenH2.Core.Maps;
using OpenH2.Core.Tags.Layout;
using OpenH2.Serialization.Layout;

namespace OpenH2.Core.Tags
{
    [TagLabel(TagName.spas)]
    public class ShaderPassTag : BaseTag
    {
        public override string Name { get; set; }
        public ShaderPassTag(uint id) : base(id)
        {
        }

        [ReferenceArray(28)]
        public Wrapper1[] Wrapper1s { get; set; }

        [FixedLength(8)]
        public class Wrapper1
        {
            [ReferenceArray(0)]
            public Wrapper2[] Wrapper2s { get; set; }
        }

        [FixedLength(330)]
        public class Wrapper2
        {
            [PrimitiveValue(256)]
            public TagRef<VertexShaderTag> VertexShaderId { get; set; }

            [ReferenceArray(306)]
            public ShaderReferenceGroup[] ShaderReferenceGroups1 { get; set; }

            [ReferenceArray(314)]
            public uint[] Somethings { get; set; }

            [ReferenceArray(322)]
            public byte[] ShaderIdsMaybe { get; set; }
        }

        [FixedLength(44)]
        public class ShaderReferenceGroup
        {
            //[InternalReferenceValue(20)]
            public byte[] ShaderData1 { get; set; }

            //[InternalReferenceValue(28)]
            public byte[] ShaderData2 { get; set; }

            //[InternalReferenceValue(36)]
            public byte[] ShaderData3 { get; set; }
        }
    }
}
