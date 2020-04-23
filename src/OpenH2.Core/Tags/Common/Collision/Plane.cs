using OpenH2.Core.Tags.Layout;
using System.Numerics;

namespace OpenH2.Core.Tags.Common.Collision
{
    [FixedLength(16)]
    public class Plane
    {
        [PrimitiveValue(0)]
        public Vector3 Normal { get; set; }

        [PrimitiveValue(12)]
        public float Distance { get; set; }
    }
}
