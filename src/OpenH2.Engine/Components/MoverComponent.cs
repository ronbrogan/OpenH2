using OpenH2.Core.Architecture;
using System.Numerics;

namespace OpenH2.Engine.Components
{
    public class MoverComponent : Component
    {
        public MovementMode Mode { get; }
        public float Speed { get; set; } = 0.1f;

        public MoverComponent(Entity parent, MovementMode mode) : base(parent)
        {
            this.Mode = mode;
        }

        public enum MovementMode
        {
            Freecam,
            CharacterControl
        }
    }
}
