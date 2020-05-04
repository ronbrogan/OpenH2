using OpenH2.Foundation.Physics;
using System.Numerics;

namespace OpenH2.Physics.Colliders
{
    public class ConvexMeshCollider : IVertexBasedCollider
    {
        private readonly ITransform xform;
        public Vector3[] Vertices { get; private set; }
        public int PhysicsMaterial => -1;

        public ConvexMeshCollider(ITransform xform, Vector3[] verts)
        {
            this.Vertices = verts;
            this.xform = xform;
        }

        public Vector3[] GetTransformedVertices()
        {
            // TODO: transform or cut method from interface
            return Vertices;
        }
    }
}
