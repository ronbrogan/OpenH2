using OpenH2.Core.Architecture;
using OpenH2.Engine.Components;
using System.Numerics;

namespace OpenH2.Engine.Entities
{
    public class SpectatorCamera : Entity
    {
        

        public SpectatorCamera()
        {
            var xform = new TransformComponent(this);

            this.Components = new Component[]
            {
                new MoverComponent(this),
                xform,
                new CameraComponent(this)
            };

            xform.Position = new Vector3(0, 0, 6);
        }

    }
}
