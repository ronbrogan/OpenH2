using OpenH2.Core.Architecture;
using OpenH2.Physics.Colliders;

namespace OpenH2.Engine.Components
{
    public class StaticTerrainComponent : Component
    {
        public StaticTerrainComponent(Entity parent) : base(parent)
        {
        }

        public TriangleMeshCollider Collider { get; internal set; }

        public object PhysicsActor { get; set; }
    }
}
