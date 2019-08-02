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

                    var rotation = new Vector3(pitch, 0, yaw);

                    UpdateMoversRot(movers, rotation);
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
                { Key.A, () => UpdateMovers(movers, new Vector3(speed, 0, 0)) },
                { Key.S, () => UpdateMovers(movers, new Vector3(0, -speed, 0)) },
                { Key.D, () => UpdateMovers(movers, new Vector3(-speed, 0, 0)) },
                { Key.LShift, () => UpdateMovers(movers, new Vector3(0, 0, speed)) },
                { Key.Space, () => UpdateMovers(movers, new Vector3(0, 0, -speed)) },
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
                    // -pi/2 is used to offset rotation from YUp to ZUp coords
                    var mat = Matrix4x4.CreateFromYawPitchRoll(xform.Orientation.Yaw(), -(float)Math.PI / 2f, 0);

                    var forward = new Vector3(mat.M13, mat.M23, mat.M33);
                    var jump = new Vector3(mat.M12, mat.M22, mat.M32);
                    var strafe = new Vector3(mat.M11, mat.M21, mat.M31);

                    var offset = (deltap.Y * forward) + (deltap.X * strafe) + (deltap.Z * jump);

                    xform.Position += offset;
                }
            }
        }

        public void UpdateMoversRot(List<MoverComponent> movers, Vector3 deltar)
        {
            foreach (var mover in movers)
            {
                if (mover.TryGetSibling<TransformComponent>(out var xform))
                {
                    xform.Orientation += deltar;
                }
            }
        }
    }
}
