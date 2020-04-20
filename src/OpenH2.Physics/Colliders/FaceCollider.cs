using OpenH2.Foundation;
using OpenH2.Foundation.Numerics;
using OpenH2.Foundation.Physics;
using OpenH2.Physics.Bounds;
using OpenH2.Physics.Colliders.Tests;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace OpenH2.Physics.Colliders
{
    public class FaceCollider : IVertexBasedCollider, IBody
    {
        private readonly Face face;
        public Vector3[] Vertices => face.Vertices;
        public Vector3 Normal => face.Normal;
        public Vector3 Position => face.Position;

        public ISweepableBounds Bounds { get; set; }
        public bool IsAwake => false;
        public bool IsStatic => true;
        public ICollider Collider => this;
        public ITransform Transform => IdentityTransform.Instance();

        public FaceCollider(Face face, Vector3 min, Vector3 max)
        {
            this.face = face;

            this.Bounds = new AxisAlignedBoundingBox(min, max);
        }

        public Vector3 Support(Vector3 direction, out float distance)
        {
            var index = Support(0, direction, out distance);
            
            var furthest = this.Vertices[index];

            if (Vector3.Dot(direction, Normal) < 0)
                furthest -= Normal;

            return furthest;
        }

        public short Support(short vertIndex, VectorD3 direction, out double distance)
        {
            var index = Support(vertIndex, direction.ToSingle(), out var fdistance);
            distance = fdistance;
            return index;
        }

        public void Wake() { }

        private short Support(short vertIndex, Vector3 direction, out float distance)
        {
            Span<float> dots = stackalloc float[this.Vertices.Length];

            for (int i = 0; i < this.Vertices.Length; i++)
            {
                dots[i] = Vector3.Dot(this.Vertices[i], direction);
            }

            if (vertIndex == -1)
                vertIndex = 0;

            float max = float.MinValue;
            short maxIndex = -1;
            for (short i = vertIndex; i < dots.Length; i++)
            {
                if (dots[i] > max)
                {
                    max = dots[i];
                    maxIndex = i;
                }
            }

            distance = max;
            return maxIndex;
        }

        public bool Intersects(ICollider other)
        {
            if(other is ISupportableCollider supportable)
            {
                return Gjk.Test(this, supportable, out var mtv);
            }
            else
            {
                return false;
            }
        }

        public IList<Contact> GenerateContacts(ICollider other)
        {
            var contacts = new List<Contact>();

            if (other is ISupportableCollider supportable && Gjk.Test(this, supportable, out var mtv))
            {
                var c = new Contact()
                {
                    Point = supportable.Position,
                    Normal = -Vector3.Normalize(mtv),
                    Penetration = mtv.Length(),
                    Friction = 1f,
                    Restitution = 0.1f
                };

                contacts.Add(c);
            }

            return contacts;
        }
    }
}
