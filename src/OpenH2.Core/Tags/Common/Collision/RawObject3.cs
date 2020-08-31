using OpenH2.Core.Tags.Layout;
using OpenH2.Serialization.Layout;

namespace OpenH2.Core.Tags.Common.Collision
{
    [FixedLength(4)]
    public class RawObject3
    {
        [PrimitiveValue(0)]
        public byte val1 { get; set; }

        [PrimitiveValue(1)]
        public byte val2 { get; set; }

        [PrimitiveValue(2)]
        public ushort FourIndex { get; set; }
    }
}
