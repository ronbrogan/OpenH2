using OpenH2.Core.Representations;
using OpenH2.Core.Tags.Layout;

namespace OpenH2.Core.Tags.Common
{
    [FixedLength(32)]
    public class ModelShaderReference
    {
        [PrimitiveValue(12)]
        public TagRef<ShaderTag> ShaderId { get; set; }

        [PrimitiveValue(20)]
        public uint Offset { get; set; }
    }
}
