using OpenH2.Core.Architecture;
using OpenH2.Foundation.Physics;

namespace OpenH2.Engine.Components
{
    public class StaticGeometryComponent : Component, IBody
    {
        public StaticGeometryComponent(Entity parent) : base(parent)
        {
        }

        public ICollider Collider { get; set; }
        public ITransform Transform { get; set; }
    }
}
