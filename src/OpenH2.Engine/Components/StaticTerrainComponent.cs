using OpenH2.Core.Architecture;
using OpenH2.Foundation;
using OpenH2.Foundation.Physics;
using OpenH2.Physics.Colliders;
using OpenH2.Physics.SpatialPartitioning;

namespace OpenH2.Engine.Components
{
    public class StaticTerrainComponent : Component, ILevelGeometry<FaceCollider>
    {
        public StaticTerrainComponent(Entity parent) : base(parent)
        {
        }

        public FaceCollider[] Collision { get; set; }
    }
}
