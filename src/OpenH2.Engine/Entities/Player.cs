using OpenH2.Core.Architecture;
using OpenH2.Engine.Components;
using System;
using System.Numerics;

namespace OpenH2.Engine.Entities
{
    public class Player : Entity
    {
        public Player(bool useDynamicController)
        {
            var xform = new TransformComponent(this, Quaternion.Identity);
            xform.Position = new Vector3(0, 0, 3);
            xform.UpdateDerivedData();

            var camera = new CameraComponent(this);

            var light = new PointLightEmitterComponent(this);
            light.Light = new Foundation.PointLight()
            {
                Position = Vector3.Zero,
                Color = new Vector3(0.3f, 1f, 0.3f),
                Radius = 20f
            };

            var moverConfig = new MoverComponent.MovementConfig()
            {
                Mode = useDynamicController 
                    ? MoverComponent.MovementMode.DynamicCharacterControl
                    : MoverComponent.MovementMode.KinematicCharacterControl,
                Speed = 0.1f,
            };

            this.Components = new Component[]
            {
                new MoverComponent(this, xform, moverConfig),
                xform,
                camera
            };
        }
    }
}
