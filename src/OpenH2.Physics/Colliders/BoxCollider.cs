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
        public Vector3 Center => transform.Position;
        public Vector3 HalfWidths { get; set; }
        public ISweepableBounds Bounds { get; set; }
        public Matrix4x4 Transform => transform.TransformationMatrix;
        private ITransform transform;

        public BoxCollider(ITransform xform, Vector3 HalfWidths)
        {
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
                return ContactGenerators.CollideBoxAndPlane(this, plane);
            }
            else if (other is BoxCollider box)
            {
                return ContactGenerators.BoxAndBox(this, box);
            }

            return ContactGenerators.Empty;
        }

        public bool Intersects(ICollider other)
        {
            if(other is PlaneCollider plane)
            {
                return IntersectionTests.BoxAndPlane(this, plane);
            } 
            else if (other is BoxCollider box)
            {
                return IntersectionTests.BoxAndBox(this, box);
            }

            return false;
        }

        public Vector3 GetAxis(int index)
        {
            var mat = Transform;

            switch (index)
            {
                case 0:
                    return new Vector3(mat.M11, mat.M12, mat.M13);
                case 1:
                    return new Vector3(mat.M21, mat.M22, mat.M23);
                case 2:
                    return new Vector3(mat.M31, mat.M32, mat.M33);
                case 3:
                    return new Vector3(mat.M41, mat.M42, mat.M43);
                default:
                    throw new Exception("Bad axis provided");
            }
        }
    }
}
