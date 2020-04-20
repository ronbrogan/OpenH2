using OpenH2.Foundation.Numerics;
using OpenH2.Foundation.Physics;
using OpenH2.Physics.Bounds;
using OpenH2.Physics.Colliders.Contacts;
using OpenH2.Physics.Colliders.Tests;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace OpenH2.Physics.Colliders
{
    public class BoxCollider : IVertexBasedCollider
    {
        private static float[,] VertMultipliers = new float[,] {{1,1,1},{-1,1,1},{1,-1,1},{-1,-1,1},{1,1,-1},{-1,1,-1},{1,-1,-1},{-1,-1,-1}};
        private ITransform transform;

        public Vector3 OriginOffset { get; }
        public Vector3 HalfWidths { get; set; }
        public ISweepableBounds Bounds { get; set; }
        public Matrix4x4 Transform => transform.TransformationMatrix;
        public Vector3 Position => transform.Position;
        public Vector3[] Vertices { get; private set; }

        public BoxCollider(ITransform xform, Vector3 HalfWidths, Vector3 originOffset = default)
        {
            this.OriginOffset = originOffset;
            this.HalfWidths = HalfWidths;
            this.transform = xform;

            // TODO: use AABB
            var radius = Math.Max(Math.Max(HalfWidths.X, HalfWidths.Y), HalfWidths.Z);
            Bounds = new SphereBounds(xform, originOffset, radius);

            Vertices = new Vector3[8];

            for (var i = 0; i < 8; i++)
            {
                // Go through each combination of + and - for each half-size
                Vertices[i] = Vector3.Multiply(new Vector3(VertMultipliers[i, 0], VertMultipliers[i, 1], VertMultipliers[i, 2]), HalfWidths);
            }
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
            else if (other is FaceCollider face)
            {
                return face.GenerateContacts(this);
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
            else if(other is FaceCollider face)
            {
                return face.Intersects(this);
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
                    return new Vector3(mat.M41, mat.M42, mat.M43) + this.OriginOffset;
                default:
                    throw new Exception("Bad axis provided");
            }
        }

        public Vector3[] GetTransformedVertices()
        {
            var verts = new Vector3[8];

            for (var i = 0; i < 8; i++)
            {
                // Transform each vertex from local space into world space
                var vertexPos = Vertices[i];
                vertexPos += OriginOffset;
                vertexPos = Vector3.Transform(vertexPos, Transform);

                verts[i] = vertexPos;
            }

            return verts;
        }

        public Vector3 Support(Vector3 direction, out float distance)
        {
            var index = Support(0, new VectorD3(direction), out var ddistance);
            distance = (float)ddistance;
            return this.GetTransformedVertices()[index];
        }

        // PERF: parallelize dot/max
        public short Support(short vertIndex, VectorD3 direction, out double distance)
        {
            var index = Support(vertIndex, direction.ToSingle(), out var fdistance);
            distance = fdistance;
            return index;
        }

        private short Support(short vertIndex, Vector3 direction, out float distance)
        {
            Span<float> dots = stackalloc float[8];

            Vector3[] verts = GetTransformedVertices();
            for (int i = 0; i < verts.Length; i++)
            {
                dots[i] = Vector3.Dot(verts[i], direction);
            }

            if (vertIndex == -1)
                vertIndex = 0;

            var max = float.MinValue;
            short maxIndex = -1;
            for (short d = vertIndex; d < dots.Length; d++)
            {
                if (dots[d] > max)
                {
                    max = dots[d];
                    maxIndex = d;
                }
            }

            if(maxIndex == -1)
            {
                throw new Exception();
            }

            distance = max;
            return maxIndex;
        }
    }
}
