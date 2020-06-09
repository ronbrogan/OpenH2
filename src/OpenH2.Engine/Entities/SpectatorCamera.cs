using OpenH2.Core.Architecture;
using OpenH2.Engine.Components;
using OpenH2.Engine.Systems.Movement;
using System;
using System.Numerics;

namespace OpenH2.Engine.Entities
{
    public class SpectatorCamera : Entity
    {
        public SpectatorCamera()
        {
            var xform = new TransformComponent(this, Quaternion.CreateFromAxisAngle(EngineGlobals.Up, MathF.PI/-2f));
            xform.Position = new Vector3(-11.05469f, 25.309626f, 4.0002475f);
            xform.UpdateDerivedData();

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
                new MoverComponent(this, xform, new MoverComponent.MovementConfig(speed: 0.1f)),
                xform,
                camera
            };
        }
    }
}
