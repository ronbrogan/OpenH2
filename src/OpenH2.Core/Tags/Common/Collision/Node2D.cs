using OpenH2.Core.Tags.Layout;
using OpenBlam.Serialization.Layout;
using System.Numerics;

namespace OpenH2.Core.Tags.Common.Collision
{
    [FixedLength(16)]
    public class Node2D
    {
        [PrimitiveValue(0)]
        public Vector2 PlaneNormal { get; set; }

        [PrimitiveValue(8)]
        public float PlaneDistance { get; set; }

        [PrimitiveValue(12)]
        public ushort Left { get; set; }

        [PrimitiveValue(14)]
        public ushort Right { get; set; }
    }
}
