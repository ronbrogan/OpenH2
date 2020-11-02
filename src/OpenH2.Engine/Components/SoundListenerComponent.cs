using OpenH2.Core.Architecture;
using System.Numerics;

namespace OpenH2.Engine.Components
{
    public class SoundListenerComponent : Component
    {
        public Vector3 PositionOffset { get; set; }
        public TransformComponent Transform { get; }

        public SoundListenerComponent(Entity parent, TransformComponent transform) : base(parent)
        {
            this.Transform = transform;
        }
    }
}
