using OpenH2.Foundation.Physics;
using OpenH2.Physics.Colliders.Contacts;
using OpenH2.Physics.Colliders.Tests;
using System.Collections.Generic;
using System.Numerics;

namespace OpenH2.Physics.Colliders
{
    public class PlaneCollider : ICollider
    {
        public Vector3 Normal { get; set; }
        public float Distance { get; set; }
        public ISweepableBounds Bounds { get; set; }

        public PlaneCollider() { }

        public IList<Contact> GenerateContacts(ICollider other)
        {
            if (other is BoxCollider box)
            {
                return ContactGenerator.CollideBoxAndPlane(box, this);
            }

            return ContactGenerator.Empty;
        }

        public bool Intersects(ICollider other)
        {
            if(other is BoxCollider box)
            {
                return IntersectionTests.BoxAndPlane(box, this);
            }

            return false;
        }
    }
}
