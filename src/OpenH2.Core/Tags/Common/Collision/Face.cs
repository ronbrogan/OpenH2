using OpenH2.Core.Tags.Layout;
using OpenBlam.Serialization.Layout;

namespace OpenH2.Core.Tags.Common.Collision
{
    [FixedLength(8)]
    public class Face
    {
        [PrimitiveValue(0)]
        public ushort val1 { get; set; }

        [PrimitiveValue(2)]
        public ushort FirstEdge { get; set; }

        [PrimitiveValue(4)]
        public byte val3 { get; set; }

        [PrimitiveValue(5)]
        public byte val3_2 { get; set; }

        [PrimitiveValue(6)]
        public ushort ShaderIndex { get; set; }
    }
}
