using OpenH2.Core.Architecture;
using System.Numerics;

namespace OpenH2.Engine.Components
{
    public class SkyLightComponent : Component
    {
        public Vector3 Direction { get; set; }


        public SkyLightComponent(Entity parent) : base(parent)
        {

        }
    }
}
