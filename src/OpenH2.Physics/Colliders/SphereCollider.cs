using OpenH2.Foundation.Physics;
using OpenH2.Physics.Colliders.Contacts;
using System.Collections.Generic;
using System.Numerics;

namespace OpenH2.Physics.Colliders
{
    public class SphereCollider : ICollider
    {
        public Vector3 Center { get; set; }
        public float Radius { get; set; }
        public ISweepableBounds Bounds { get; set; }
        public Matrix4x4 Transform { get; set; }

        public IList<Contact> GenerateContacts(ICollider other)
        {
            return ContactGenerators.Empty;
        }

        public bool Intersects(ICollider other)
        {
            return false;
        }
    }
}
