using OpenH2.Core.Architecture;
using OpenH2.Foundation.Physics;
using System.Numerics;

namespace OpenH2.Engine.Components
{
    public class StaticGeometryComponent : Component
    {
        public Vector3[] Vertices { get; set; }

        /// <summary>
        /// Array of vertex indices, where each three items constitutes a single triangle
        /// </summary>
        public int[] TriangleIndices { get; set; }

        /// <summary>
        /// Array of material indices, where each item is the given triangles global material index
        /// </summary>
        public int[] MaterialIndices { get; set; }

        public TransformComponent Transform { get; }

        public StaticGeometryComponent(Entity parent, TransformComponent xform) : base(parent)
        {
            this.Transform = xform;
        }
    }
}
