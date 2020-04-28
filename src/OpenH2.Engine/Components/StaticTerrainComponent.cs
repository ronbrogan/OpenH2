using OpenH2.Core.Architecture;
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
