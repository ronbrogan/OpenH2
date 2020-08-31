using OpenH2.Core.Tags.Layout;
using OpenH2.Serialization.Layout;

namespace OpenH2.Core.Tags.Common.Collision
{
    [FixedLength(8)]
    public class Node3D
    {
        [PrimitiveValue(0)]
        public ushort PlaneIndex { get; set; }

        [PrimitiveValue(2)]
        public ushort Left { get; set; }

        [PrimitiveValue(4)]
        public byte val1 { get; set; }

        [PrimitiveValue(5)]
        public ushort Right { get; set; }

        [PrimitiveValue(7)]
        public byte val2 { get; set; }
    }
}
