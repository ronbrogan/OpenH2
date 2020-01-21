using OpenH2.Foundation.Physics;
using OpenH2.Physics.Bounds;
using OpenH2.Physics.Colliders.Contacts;
using OpenH2.Physics.Colliders.Tests;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace OpenH2.Physics.Colliders
{
    public class BoxCollider : ICollider
    {
        public Vector3 Center { get; set; }
        public Vector3 HalfWidths { get; set; }
        public ISweepableBounds Bounds { get; set; }
        public Matrix4x4 Transform => transform.TransformationMatrix;
        private ITransform transform;

        public BoxCollider(ITransform xform, Vector3 Center, Vector3 HalfWidths)
        {
            this.Center = Center;
            this.HalfWidths = HalfWidths;
            this.transform = xform;

            // TODO: use AABB
            var radius = Math.Max(Math.Max(HalfWidths.X, HalfWidths.Y), HalfWidths.Z);
            Bounds = new SphereBounds(xform, Center, radius);
        }



        public IList<Contact> GenerateContacts(ICollider other)
        {
            if (other is PlaneCollider plane)
            {
                return ContactGenerator.CollideBoxAndPlane(this, plane);
            }

            return ContactGenerator.Empty;
        }

        public bool Intersects(ICollider other)
        {
            if(other is PlaneCollider plane)
            {
                return IntersectionTests.BoxAndPlane(this, plane);
            }


            return false;
        }
    }
}
