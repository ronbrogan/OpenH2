using OpenH2.Core.Architecture;
using OpenH2.Foundation;

namespace OpenH2.Engine.Components
{
    public class PointLightEmitterComponent : Component
    {
        public PointLight Light { get; set; }

        public PointLightEmitterComponent(Entity parent) : base(parent)
        {
        }
    }
}
