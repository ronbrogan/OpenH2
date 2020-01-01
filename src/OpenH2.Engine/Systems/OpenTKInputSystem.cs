using OpenH2.Core.Architecture;
using OpenH2.Core.Extensions;
using OpenH2.Engine.Components;
using OpenH2.Engine.Stores;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace OpenH2.Engine.Systems
{
    public class OpenTKInputSystem : WorldSystem
    {
        public OpenTKInputSystem(World world) : base(world)
        {
        }

        public override void Update(double timestep)
        {
            var inputs = this.world.GetGlobalResource<InputStore>();
            var movers = this.world.Components<MoverComponent>();

            var mouse = Mouse.GetCursorState();
            var currPos = new Vector2(mouse.X, mouse.Y);
            if (mouse.LeftButton == ButtonState.Pressed)
            {
                if (inputs.MousePos == default(Vector2))
                {
                    inputs.MousePos = currPos;
                }
                else
                {
                    var mouse_delta = inputs.MousePos - currPos;

                    const float mouseX_Sensitivity = -0.005f;
                    const float mouseY_Sensitivity = -0.005f;

                    var yaw = mouseX_Sensitivity * mouse_delta.X;
                    var pitch = mouseY_Sensitivity * mouse_delta.Y;

                    UpdateMoversRot(movers, yaw, pitch);
                }
            }
            inputs.MousePos = currPos;


            var kb = Keyboard.GetState();

            var speed = 0.1f;

            if(kb[Key.LControl])
            {
                speed = 1.0f;
            }

            var keyMap = new Dictionary<Key, Action>
            {
                { Key.W, () => UpdateMovers(movers, new Vector3(0, speed, 0)) },
                { Key.S, () => UpdateMovers(movers, new Vector3(0, -speed, 0)) },
                { Key.A, () => UpdateMovers(movers, new Vector3(speed, 0, 0)) },
                { Key.D, () => UpdateMovers(movers, new Vector3(-speed, 0, 0)) },
                { Key.Space, () => UpdateMovers(movers, new Vector3(0, 0, speed)) },
                { Key.LShift, () => UpdateMovers(movers, new Vector3(0, 0, -speed)) },
            };
            
            foreach(var key in keyMap.Keys)
            {
                if (kb[key])
                {
                    keyMap[key]();
                }
            }
        }

        public void UpdateMovers(List<MoverComponent> movers, Vector3 deltap)
        {
            foreach(var mover in movers)
            {
                if(mover.TryGetSibling<TransformComponent>(out var xform))
                {
                    var totalOrientation = Quaternion.Normalize(xform.Orientation);

                    var forward = Vector3.Transform(new Vector3(0,1,0), totalOrientation);
                    forward = Vector3.Normalize(new Vector3(forward.X, forward.Z, 0));

                    var up = new Vector3(0, 0, 1);
                    var strafe = Vector3.Cross(forward, up);

                    var offset = (deltap.Y * -forward) + (deltap.X * strafe) + (deltap.Z * up);

                    xform.Position += offset;
                }
            }
        }

        public void UpdateMoversRot(List<MoverComponent> movers, float yaw, float pitch)
        {
            var yawQuat = Quaternion.CreateFromAxisAngle(new Vector3(0, 0, 1), yaw);
            var pitchQuat = Quaternion.CreateFromAxisAngle(new Vector3(1, 0, 0), pitch);

            foreach (var mover in movers)
            {
                if (mover.TryGetSibling<TransformComponent>(out var xform))
                {
                    xform.Orientation = Quaternion.Normalize(pitchQuat * xform.Orientation * yawQuat);
                }
            }
        }
    }
}
