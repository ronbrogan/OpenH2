using OpenH2.Foundation.Physics;
using System.Numerics;

namespace OpenH2.Physics.Colliders
{
    public class PlaneCollider : ICollider
    {
        public Vector3 Normal { get; set; }
        public float Distance { get; set; }

        public PlaneCollider() { }
    }
}
