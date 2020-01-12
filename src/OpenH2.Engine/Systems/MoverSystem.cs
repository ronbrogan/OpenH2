using OpenH2.Core.Architecture;
using OpenH2.Engine.Components;
using OpenH2.Engine.Stores;
using OpenTK.Input;
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
                const float mouseX_Sensitivity = -0.005f;
                const float mouseY_Sensitivity = -0.005f;

                yaw = mouseX_Sensitivity * input.MouseDiff.X;
                pitch = mouseY_Sensitivity * input.MouseDiff.Y;

            }

            var deltaP = GetPositionDelta(input);
            
            UpdateMovers(movers, deltaP, yaw, pitch);
        }

        private Vector3 GetPositionDelta(InputStore input)
        {
            var speed = 0.1f;

            var kb = input.Keyboard;

            if (kb[Key.LControl])
            {
                speed = 1.0f;
            }

            var delta = Vector3.Zero;

            var keyMap = new Dictionary<Key, Action>
            {
                { Key.W, () => delta += new Vector3(0, speed, 0) },
                { Key.S, () => delta += new Vector3(0, -speed, 0) },
                { Key.A, () => delta += new Vector3(speed, 0, 0) },
                { Key.D, () => delta += new Vector3(-speed, 0, 0) },
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

        public void UpdateMovers(List<MoverComponent> movers, Vector3 deltap, float yaw, float pitch)
        {
            var yawQuat = Quaternion.CreateFromAxisAngle(new Vector3(0, 0, 1), yaw);
            var pitchQuat = Quaternion.CreateFromAxisAngle(new Vector3(1, 0, 0), pitch);

            foreach (var mover in movers)
            {
                if (mover.TryGetSibling<TransformComponent>(out var xform))
                {
                    xform.Orientation = Quaternion.Normalize(pitchQuat * xform.Orientation * yawQuat);
                    var totalOrientation = Quaternion.Normalize(xform.Orientation);

                    var forward = Vector3.Transform(new Vector3(0, 1, 0), totalOrientation);
                    forward = Vector3.Normalize(new Vector3(forward.X, forward.Z, 0));

                    var up = new Vector3(0, 0, 1);
                    var strafe = Vector3.Cross(forward, up);

                    var offset = (deltap.Y * -forward) + (deltap.X * strafe) + (deltap.Z * up);

                    xform.Position += offset;
                }
            }
        }
    }
}
