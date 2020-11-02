using OpenH2.Audio.Abstractions;
using OpenH2.Core.Architecture;

namespace OpenH2.Engine.Components
{
    public class SoundEmitterComponent : Component
    {
        public ISoundEmitter Emitter { get; set; }
        public TransformComponent Transform { get; }

        public SoundEmitterComponent(Entity parent, TransformComponent xform) : base(parent)
        {
            this.Transform = xform;
        }
    }
}
