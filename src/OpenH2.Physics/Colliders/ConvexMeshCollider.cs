using OpenH2.Foundation.Physics;
using System;
using System.Numerics;

namespace OpenH2.Physics.Colliders
{
    public class ConvexMeshCollider : IVertexBasedCollider
    {
        public Vector3[] Vertices => throw new NotImplementedException();

        public Vector3[] GetTransformedVertices()
        {
            throw new NotImplementedException();
        }
    }
}
