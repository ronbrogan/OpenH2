using OpenH2.Core.Architecture;
using System.Numerics;

namespace OpenH2.Engine.Components
{
    public class MoverComponent : Component
    {
        public TransformComponent Transform { get; }
        public MovementMode Mode { get; }
        public float Speed { get; set; } = 0.1f;
        public Vector3 DisplacementAccumulator { get; set; }

        public MoverComponent(Entity parent, TransformComponent xform, MovementMode mode) : base(parent)
        {
            this.Transform = xform;
            this.Mode = mode;
        }

        public enum MovementMode
        {
            Freecam,
            CharacterControl
        }
    }
}
