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
            var xform = new TransformComponent(this)
            {
                Orientation = Quaternion.CreateFromYawPitchRoll(0, (float)Math.PI / -2f, 0)
            };
            var camera = new CameraComponent(this);

            var light = new PointLightEmitterComponent(this);
            light.Light = new Foundation.PointLight()
            {
                Position = Vector3.Zero,
                Color = new Vector3(0.3f, 1f, 0.3f),
                Radius = 20f
            };

            this.Components = new Component[]
            {
                new MoverComponent(this),
                xform,
                camera,
                //light
            };

            xform.Position = new Vector3(0,0,10);
        }

    }
}
