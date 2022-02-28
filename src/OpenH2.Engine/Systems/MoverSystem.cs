using OpenH2.Core.Architecture;
using OpenH2.Core.Extensions;
using OpenH2.Engine.Components;
using OpenH2.Engine.Stores;
using OpenH2.Engine.Systems.Movement;
using OpenH2.Foundation.Extensions;
using OpenH2.Foundation.Logging;
using OpenTK.Windowing.Common.Input;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Silk.NET.Input;
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

            UpdateMovers(movers, input, yaw, pitch, timestep);
        }

        private static Dictionary<Key, Func<float, Vector3, Vector3>> downKeyMap = new()
        {
            { Key.W, (s,d) => d += new Vector3(s, 0, 0) },
            { Key.S, (s,d) => d += new Vector3(-s, 0, 0) },
            { Key.A, (s,d) => d += new Vector3(0, -s, 0) },
            { Key.D, (s,d) => d += new Vector3(0, s, 0) },
            { Key.ShiftLeft, (s,d) => d += new Vector3(0, 0, -s) },
        };

        private Vector3 GetInput(InputStore input, MoverComponent mover)
        {
            var speed = 1f;

            if (input.IsDown(Key.ControlLeft))
            {
                speed = 10.0f;
            }

            var delta = Vector3.Zero;

            // handle down Key
            foreach (var (key, _) in downKeyMap)
            {
                if (input.IsDown(key))
                {
                    delta = downKeyMap[key](speed, delta);
                }
            }

            if(input.WasPressed(Key.M))
            {
                if(mover.Mode != mover.Config.Mode)
                {
                    // If previously toggled, switch back to original mode
                    mover.Mode = mover.Config.Mode;
                }
                else
                {
                    // Otherwise, set to what the inverse of the configured mode is
                    if (mover.Config.Mode == MoverComponent.MovementMode.Freecam)
                    {
                        mover.Mode = MoverComponent.MovementMode.DynamicCharacterControl;
                    }
                    else
                    {
                        mover.Mode = MoverComponent.MovementMode.Freecam;
                    }
                }

                // TODO: implment a way to update the physics implementation without remove/add
                this.world.Scene.RemoveEntity(mover.Parent);
                this.world.Scene.AddEntity(mover.Parent);
            }

            if(input.WasPressed(Key.P))
            {
                var p = mover.Transform.Position;
                var q = mover.Transform.Orientation;
                var roll = MathF.Atan2(2.0f * (q.Z * q.Y + q.W * q.X), 1.0f - 2.0f * (q.X * q.X + q.Y * q.Y));
                var pitch = MathF.Asin(2.0f * (q.Y * q.W - q.Z * q.X));
                var yaw = MathF.Atan2(2.0f * (q.Z * q.W + q.X * q.Y), -1.0f + 2.0f * (q.W * q.W + q.X * q.X));

                Logger.Log($"Current Location: {p.X.ToString("0.00")},{p.Y.ToString("0.00")},{p.Z.ToString("0.00")}@{yaw.ToString("0.0000")}", Logger.Color.White);
            }

            if(mover.Mode == MoverComponent.MovementMode.Freecam)
            {
                if(input.IsDown(Key.Space))
                {
                    delta += new Vector3(0, 0, speed);
                }
            }
            else
            {
                if (input.WasPressed(Key.Space))
                {
                    delta += new Vector3(0, 0, speed);
                }
            }

            return delta;
        }

        public void UpdateMovers(List<MoverComponent> movers, InputStore input, float yaw, float pitch, double timestep)
        {
            var yawQuat = Quaternion.CreateFromAxisAngle(EngineGlobals.Up, -yaw);
            var pitchQuat = Quaternion.CreateFromAxisAngle(EngineGlobals.Strafe, -pitch);

            foreach (var mover in movers)
            {
                var xform = mover.Transform;
                
                // Update camera orientation
                xform.Orientation = Quaternion.Normalize(yawQuat * xform.Orientation * pitchQuat);

                var inputVector = GetInput(input, mover);

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
                    mover.PhysicsImplementation.Move(offset, timestep);
                }
                else if(mover.Mode == MoverComponent.MovementMode.DynamicCharacterControl)
                {
                    UpdateDynamicController(mover, mover.State as DynamicMovementController, moverInputVector, forward, strafe, timestep);
                }
            }
        }

        /// <summary>
        /// Goals of movement
        ///  - Foot friction
        ///  - Inertia (quick reversal of direction causes lag in movement)
        ///  - There's a "sliding" state where any desired movement along the velocity vector
        ///      (whether forward or backward) is not taken, however lateral (with respect to the current velocity)
        ///      movement is honored
        ///  - Maximum climb angle, slide if above
        ///  - Slide can happen from going too fast to get traction
        ///  - Jump works as long as you could typically walk
        ///  - Friction against walls and ceilings does not slow movement
        ///  - Crouching reduces friction (causes more sliding)
        /// </summary>

        private void UpdateDynamicController(MoverComponent mover, 
            DynamicMovementController dynamic, 
            Vector3 inputVector, 
            Vector3 forward, 
            Vector3 strafe, 
            double timestep)
        {
            dynamic.Move(mover.PhysicsImplementation, inputVector, forward, strafe);
        }
    }
}
