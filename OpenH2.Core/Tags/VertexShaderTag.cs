using OpenH2.Core.Tags.Layout;

namespace OpenH2.Core.Tags
{
    [TagLabel("vrtx")]
    public class VertexShaderTag : BaseTag
    {
        public override string Name { get; set; }

        public VertexShaderTag(uint id) : base(id)
        {
        }

        [InternalReferenceValue(4)]
        public ShaderReference[] Shaders { get; set; }


        [FixedLength(20)]
        public class ShaderReference
        {
            [InternalReferenceValue(4)]
            public byte[] ShaderData { get; set; }
        }
    }
}
