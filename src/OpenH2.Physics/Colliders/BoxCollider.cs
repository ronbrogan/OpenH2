using OpenH2.Foundation.Physics;
using System;
using System.Numerics;

namespace OpenH2.Physics.Colliders
{
    public class BoxCollider : IVertexBasedCollider
    {
        private static float[,] VertMultipliers = new float[,] {{1,1,1},{-1,1,1},{1,-1,1},{-1,-1,1},{1,1,-1},{-1,1,-1},{1,-1,-1},{-1,-1,-1}};
        private ITransform transform;

        public Vector3 OriginOffset { get; }
        public Vector3 HalfWidths { get; set; }
        public Matrix4x4 Transform => transform.TransformationMatrix;
        public Vector3 Position => transform.Position;
        public Vector3[] Vertices { get; private set; }
        public int PhysicsMaterial => -1;

        public BoxCollider(ITransform xform, Vector3 HalfWidths, Vector3 originOffset = default)
        {
            this.OriginOffset = originOffset;
            this.HalfWidths = HalfWidths;
            this.transform = xform;

            // TODO: use AABB
            var radius = Math.Max(Math.Max(HalfWidths.X, HalfWidths.Y), HalfWidths.Z);

            Vertices = new Vector3[8];

            for (var i = 0; i < 8; i++)
            {
                // Go through each combination of + and - for each half-size
                var v = Vector3.Multiply(new Vector3(VertMultipliers[i, 0], VertMultipliers[i, 1], VertMultipliers[i, 2]), HalfWidths);
                Vertices[i] = v + originOffset;
            }
        }

        public Vector3[] GetTransformedVertices()
        {
            var verts = new Vector3[8];

            for (var i = 0; i < 8; i++)
            {
                // Transform each vertex from local space into world space
                verts[i] = Vector3.Transform(Vertices[i], Transform);
            }

            return verts;
        }
    }
}
