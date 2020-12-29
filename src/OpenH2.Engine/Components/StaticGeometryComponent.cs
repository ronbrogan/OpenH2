using OpenH2.Core.Architecture;
using OpenH2.Foundation.Physics;
using OpenH2.Physics.Colliders;

namespace OpenH2.Engine.Components
{
    public class StaticGeometryComponent : Component
    {
        public TransformComponent Transform { get; }
        public ICollider Collider { get; internal set; }
        public object PhysicsActor { get; set; }

        public StaticGeometryComponent(Entity parent, TransformComponent xform) : base(parent)
        {
            this.Transform = xform;
        }
    }
}
