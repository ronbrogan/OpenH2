using OpenH2.Core.Representations;
using OpenH2.Core.Tags.Layout;

namespace OpenH2.Core.Tags
{
    [TagLabel("spas")]
    public class ShaderPassTag : BaseTag
    {
        public override string Name { get; set; }
        public ShaderPassTag(uint id) : base(id)
        {
        }

        [InternalReferenceValue(28)]
        public Wrapper1[] Wrapper1s { get; set; }

        [FixedLength(8)]
        public class Wrapper1
        {
            [InternalReferenceValue(0)]
            public Wrapper2[] Wrapper2s { get; set; }
        }

        [FixedLength(330)]
        public class Wrapper2
        {
            [PrimitiveValue(256)]
            public TagRef<VertexShaderTag> VertexShaderId { get; set; }

            [InternalReferenceValue(306)]
            public ShaderReferenceGroup[] ShaderReferenceGroups1 { get; set; }

            [InternalReferenceValue(314)]
            public uint[] Somethings { get; set; }

            [InternalReferenceValue(322)]
            public byte[] ShaderIdsMaybe { get; set; }
        }

        [FixedLength(44)]
        public class ShaderReferenceGroup
        {
            [InternalReferenceValue(20)]
            public byte[] ShaderData1 { get; set; }

            [InternalReferenceValue(28)]
            public byte[] ShaderData2 { get; set; }

            [InternalReferenceValue(36)]
            public byte[] ShaderData3 { get; set; }
        }
    }
}
