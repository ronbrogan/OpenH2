using OpenH2.Core.Architecture;
using OpenH2.Core.Extensions;
using OpenH2.Engine.Components;
using OpenH2.Engine.Stores;
using OpenToolkit.Windowing.Common.Input;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace OpenH2.Engine.Systems
{
    public class MoverSystem : WorldSystem
    {
        public MoverSystem(World world) : base(world)
        {
        }

        public override void Update(double timestep)
        {
            var input = this.world.GetGlobalResource<InputStore>();
            var movers = this.world.Components<MoverComponent>();

            var yaw = 0f;
            var pitch = 0f;

            if (input.MouseDown)
            {
                const float mouseX_Sensitivity = 0.005f;
                const float mouseY_Sensitivity = 0.005f;

                yaw = mouseX_Sensitivity * input.MouseDiff.X;
                pitch = mouseY_Sensitivity * input.MouseDiff.Y;

            }

            var inputVector = GetInput(input);

            UpdateMovers(movers, inputVector, yaw, pitch, timestep);
        }

        private Vector3 GetInput(InputStore input)
        {
            var speed = 1f;

            var kb = input.Keyboard;

            if (kb[Key.LControl])
            {
                speed = 10.0f;
            }

            var delta = Vector3.Zero;

            var keyMap = new Dictionary<Key, Action>
            {
                { Key.W, () => delta += new Vector3(speed, 0, 0) },
                { Key.S, () => delta += new Vector3(-speed, 0, 0) },
                { Key.A, () => delta += new Vector3(0, -speed, 0) },
                { Key.D, () => delta += new Vector3(0, speed, 0) },
                { Key.Space, () => delta += new Vector3(0, 0, speed) },
                { Key.LShift, () => delta += new Vector3(0, 0, -speed) },
            };

            foreach (var key in keyMap.Keys)
            {
                if (kb[key])
                {
                    keyMap[key]();
                }
            }

            return delta;
        }

        public void UpdateMovers(List<MoverComponent> movers, Vector3 inputVector, float yaw, float pitch, double timestep)
        {
            var yawQuat = Quaternion.CreateFromAxisAngle(EngineGlobals.Up, yaw);
            var pitchQuat = Quaternion.CreateFromAxisAngle(EngineGlobals.Strafe, pitch);

            foreach (var mover in movers)
            {
                var xform = mover.Transform;
                
                // Update camera orientation
                xform.Orientation = Quaternion.Normalize(yawQuat * xform.Orientation * pitchQuat);

                if (inputVector.LengthSquared() == 0)
                    return;

                var moverInputVector = Vector3.Multiply(inputVector, mover.Config.Speed);

                var forward = Vector3.Transform(EngineGlobals.Forward, xform.Orientation);
                var strafe = Vector3.Transform(EngineGlobals.Strafe, xform.Orientation);

                if (mover.Mode == MoverComponent.MovementMode.KinematicCharacterControl)
                {
                    // TODO: gravity, etc
                    var offset = (moverInputVector.X * forward) + (moverInputVector.Y * strafe) + (moverInputVector.Z * EngineGlobals.Up);
                    mover.PhysicsImplementation.Move(offset, timestep);
                }
                else if (mover.Mode == MoverComponent.MovementMode.Freecam)
                {
                    forward.Z = 0;
                    var offset = (moverInputVector.X * forward) + (moverInputVector.Y * strafe) + (moverInputVector.Z * EngineGlobals.Up);
                    xform.Position += offset;
                }
                else if(mover.Mode == MoverComponent.MovementMode.DynamicCharacterControl)
                {
                    UpdateDynamicController(mover, moverInputVector, forward, timestep);
                }
            }
        }

        private void UpdateDynamicController(MoverComponent mover, Vector3 inputVector, Vector3 forward, double timestep)
        {
            // TODO - config
            var accelerationRate = 50f;
            var jumpSpeed = 3f;

            // HACK: boosting input magnitude to get movement as desired
            inputVector = Vector3.Multiply(inputVector, 20f);

            var grounded = false;
            var groundNormal = Vector3.Zero;
            int doJump = 0;

            var footResults = mover.PhysicsImplementation.Raycast(-EngineGlobals.Up, 1f, 1);
            if(footResults.Length > 0)
            {
                grounded = true;
                groundNormal = footResults[0].Normal;
            }

            if (grounded)
            {
                // align our movement vectors with the ground normal (ground normal = 'up')
                Vector3 newForward = forward;
                VectorExtensions.OrthoNormalize(ref groundNormal, ref newForward);

                var deltaV = Vector3.Zero;

                Vector3 targetSpeed = newForward * inputVector.X + Vector3.Cross(newForward, groundNormal) * inputVector.Y;
                var currentVelocity = mover.PhysicsImplementation.GetVelocity();
                var squaredDiff = targetSpeed.LengthSquared() - currentVelocity.LengthSquared();

                if (squaredDiff > 0)
                {
                    float magDiff = MathF.Sqrt(squaredDiff);

                    // scale velocity gap by magDiff / acceleration
                    magDiff = 1f / magDiff;
                    var factor = magDiff;//magDiff * acceleration;

                    var velocityGap = Vector3.Subtract(targetSpeed, currentVelocity);

                    // Scale 
                    deltaV = Vector3.Multiply(velocityGap, factor);
                }

                doJump = inputVector.Z > 0f ? 1 : doJump;
                if (doJump == 1)
                { 
                    deltaV.Z = jumpSpeed - currentVelocity.Z;
                    doJump = 2;
                }

                mover.PhysicsImplementation.AddVelocity(deltaV);
            }
        }
    }
}
