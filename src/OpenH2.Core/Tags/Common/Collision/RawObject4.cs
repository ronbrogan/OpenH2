using OpenH2.Core.Tags.Layout;
using OpenH2.Serialization.Layout;

namespace OpenH2.Core.Tags.Common.Collision
{
    [FixedLength(4)]
    public class RawObject4
    {
        [PrimitiveValue(0)]
        public ushort PlaneIndex { get; set; }

        [PrimitiveValue(2)]
        public ushort Node2DIndex { get; set; }
    }
}
