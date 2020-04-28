using OpenH2.Core.Architecture;

namespace OpenH2.Engine.Components
{
    public class OriginalTagComponent : Component
    {
        public OriginalTagComponent(Entity parent, object originalTag) : base(parent)
        {
            this.OriginalTag = originalTag;
        }

        public object OriginalTag { get; }
    }
}
