using OpenH2.Core.Architecture;
using OpenH2.Engine.Components;
using System;
using System.Numerics;

namespace OpenH2.Engine.Entities
{
    public class SpectatorCamera : Entity
    {
        

        public SpectatorCamera()
        {
            var xform = new TransformComponent(this);
            var camera = new CameraComponent(this);

            this.Components = new Component[]
            {
                new MoverComponent(this),
                xform,
                camera
            };

            xform.Position = new Vector3(0, 0, -20);
            camera.OrientationOffset = new Vector3((float)Math.PI / -2f, 0, 0);
            
        }

    }
}
