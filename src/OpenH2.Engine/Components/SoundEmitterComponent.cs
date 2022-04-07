using OpenH2.Core.Architecture;
using OpenH2.Core.Audio.Abstractions;

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
