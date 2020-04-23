using OpenH2.Core.Tags.Layout;

namespace OpenH2.Core.Tags.Common.Collision
{
    [FixedLength(16)]
    public class Vertex
    {
        [PrimitiveValue(0)]
        public float x { get; set; }

        [PrimitiveValue(4)]
        public float y { get; set; }

        [PrimitiveValue(8)]
        public float z { get; set; }

        [PrimitiveValue(12)]
        public int edge { get; set; }
    }
}
