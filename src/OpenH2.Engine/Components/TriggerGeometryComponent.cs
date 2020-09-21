using OpenH2.Core.Architecture;
using OpenH2.Foundation.Physics;
using System.Numerics;

namespace OpenH2.Engine.Components
{
    public enum TriggerGeometryShape
    {
        Cuboid
    }

    public class TriggerGeometryComponent : Component
    {
        public TriggerGeometryShape Shape { get; }
        public string Name { get; private set; }
        public ITransform Transform { get; private set; }
        public Vector3 Size { get; private set; }

        private TriggerGeometryComponent(Entity parent, TriggerGeometryShape shape) : base(parent)
        {
            this.Shape = shape;
        }

        public static TriggerGeometryComponent Cuboid(Entity parent, ITransform xform, Vector3 size, string name)
        {
            var c = new TriggerGeometryComponent(parent, TriggerGeometryShape.Cuboid)
            {
                Size = size,
                Transform = xform,
                Name = name
            };

            return c;
        }
    }
}
