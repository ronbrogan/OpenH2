using OpenH2.Foundation.Physics;
using System.Collections.Generic;
using System.Numerics;

namespace OpenH2.Physics.Colliders
{
    /// <summary>
    /// A collider that is comprised of multiple convex meshes
    /// </summary>
    public class ConvexModelCollider : ICollider
    {
        public List<Vector3[]> Meshes { get; private set; }
        public int PhysicsMaterial => -1;

        public ConvexModelCollider(List<Vector3[]> vertexCollections)
        {
            this.Meshes = vertexCollections;
        }
    }
}
