using OpenH2.Foundation.Physics;
using System.Numerics;

namespace OpenH2.Physics.Colliders
{
    public class SphereCollider : ICollider
    {
        public Vector3 Center { get; set; }
        public float Radius { get; set; }
        public Matrix4x4 Transform { get; set; }
    }
}
