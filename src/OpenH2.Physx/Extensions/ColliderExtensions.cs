using OpenH2.Physics.Colliders;
using PhysX;
using System;

namespace OpenH2.Physx.Extensions
{
    public static class ColliderExtensions
    {
        public static TriangleMeshDesc GetDescriptor(this TriangleMeshCollider collider, Func<int[], short[]> materialIndexTranslator)
        {
            return new TriangleMeshDesc()
            {
                Points = collider.Vertices,
                Triangles = collider.TriangleIndices,
                MaterialIndices = materialIndexTranslator(collider.MaterialIndices)
            };
        }
    }
}
