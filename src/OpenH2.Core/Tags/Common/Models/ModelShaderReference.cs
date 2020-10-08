using OpenH2.Core.Maps;
using OpenH2.Core.Tags.Layout;
using OpenH2.Serialization.Layout;

namespace OpenH2.Core.Tags.Common.Models
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
