using OpenH2.Core.Architecture;
using OpenH2.Foundation;
using OpenH2.Foundation.Physics;
using OpenH2.Physics.Colliders;
using OpenH2.Physics.SpatialPartitioning;
using System.Numerics;

namespace OpenH2.Engine.Components
{
    public class StaticTerrainComponent : Component
    {
        public StaticTerrainComponent(Entity parent) : base(parent)
        {
        }

        public Vector3[] Vertices { get; set; }

        /// <summary>
        /// Array of vertex indices, where each three items constitutes a single triangle
        /// </summary>
        public int[] TriangleIndices { get; set; }
    }
}
