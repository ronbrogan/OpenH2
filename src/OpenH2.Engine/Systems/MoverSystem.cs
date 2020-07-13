using OpenH2.Core.Architecture;
using OpenH2.Core.Extensions;
using OpenH2.Engine.Components;
using OpenH2.Engine.Stores;
using OpenH2.Engine.Systems.Movement;
using OpenH2.Foundation.Extensions;
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

            var kb = input.DownKeys;

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
                //{ Key.Space, () => delta += new Vector3(0, 0, speed) },
                { Key.LShift, () => delta += new Vector3(0, 0, -speed) },
            };

            // handle down keys
            foreach (var key in keyMap.Keys)
            {
                if (kb[key])
                {
                    keyMap[key]();
                }
            }

            if(input.PressedKeys[Key.Space])
            {
                delta += new Vector3(0, 0, speed);
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
