using OpenH2.Core.Tags.Layout;
using OpenBlam.Serialization.Layout;

namespace OpenH2.Core.Tags.Common.Collision
{
    [FixedLength(12)]
    public class HalfEdgeContainer
    {
        [PrimitiveValue(0)]
        public ushort Vertex0 { get; set; }

        [PrimitiveValue(2)]
        public ushort Vertex1 { get; set; }

        [PrimitiveValue(4)]
        public ushort NextEdge { get; set; }

        [PrimitiveValue(6)]
        public ushort PrevEdge { get; set; }

        [PrimitiveValue(8)]
        public ushort Face0 { get; set; }

        [PrimitiveValue(10)]
        public ushort Face1 { get; set; }
    }
}
