using System.Numerics;

namespace OpenH2.Foundation.Physics
{
    public interface IVertexBasedCollider : ICollider
    {
        /// <summary>
        /// Gets un-transformed vertices (AKA in local space)
        /// </summary>
        Vector3[] Vertices { get; }

        Vector3[] GetTransformedVertices();
    }
}
