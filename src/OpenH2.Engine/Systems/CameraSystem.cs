using OpenH2.Core.Architecture;
using OpenH2.Core.GameObjects;
using OpenH2.Core.Tags;
using OpenH2.Engine.Components;
using OpenH2.Rendering.Abstractions;
using System;
using System.Linq;
using System.Numerics;

namespace OpenH2.Engine.Systems
{
    public class CameraSystem : WorldSystem
    {
        private Vector3 cameraMoveDestination = Vector3.Zero;
        private Quaternion cameraOrientationDestination = Quaternion.Identity;
        private int cameraMoveTicks = 0;

        private float desiredFovRadians = -1f;
        private int fovChangeTicks = 0;

        private readonly IGraphicsHost graphics;

        public CameraSystem(World world, IGraphicsHost graphics) : base(world)
        {
            this.graphics = graphics;
        }

        public override void Update(double timestep)
        {
            var cameras = this.world.Components<CameraComponent>();

            foreach(var camera in cameras)
            {
                if(camera.MatchViewportAspectRatio && graphics.AspectRatioChanged)
                {
                    camera.AspectRatio = graphics.AspectRatio;
                }

                if(camera.TryGetSibling<TransformComponent>(out var xform))
                {
                    UpdateViewMatrix(camera, xform);
                }

                ProcessCameraChanges(camera);

                if(camera.Dirty)
                {
                    UpdateProjectionMatrix(camera);
                }
            }
        }

        public void PerformCameraMove(AnimationGraphTag animationTag, string trackName, IUnit unit, ILocationFlag locationFlag)
        {
            var animation = animationTag.Animations.FirstOrDefault(t => t.Description == trackName);

            if (animation != null)
            {
                this.cameraMoveTicks = animation.FrameCount;
            }
        }

        public void PerformCameraMove(ICameraPathTarget destination, int tickDuration)
        {
            this.cameraMoveTicks = tickDuration;

            // TODO: either FOV data is wrong, or this isn't supposed to also do FOV changes
            //this.SetFieldOfView(destination.FieldOfView, tickDuration);
        }

        public void SetFieldOfView(float degrees, int ticks)
        {
            this.desiredFovRadians = (MathF.PI / 180) * degrees;
            this.fovChangeTicks = ticks;
        }

        public int GetCameraMoveRemaining()
        {
            return Math.Max(cameraMoveTicks, fovChangeTicks);
        }

        private void ProcessCameraChanges(CameraComponent camera)
        {
            ProcessFovChange(camera);
            ProcessCameraMove(camera);
        }

        private void ProcessFovChange(CameraComponent camera)
        {
            if (desiredFovRadians > 0)
            {
                if(fovChangeTicks == 0)
                {
                    camera.FieldOfView = desiredFovRadians;
                    desiredFovRadians = -1f;
                    return;
                }

                var delta = camera.FieldOfView - desiredFovRadians;

                delta /= fovChangeTicks;

                if(fovChangeTicks == 0)
                {
                    camera.FieldOfView = delta;
                    desiredFovRadians = -1f;
                }
                else
                {
                    camera.FieldOfView -= delta;
                }

                fovChangeTicks--;
            }
        }

        private void ProcessCameraMove(CameraComponent camera)
        {
            if (cameraMoveTicks > 0)
            {
                // TODO: move interpolation

                cameraMoveTicks--;
            }
        }

        private void UpdateViewMatrix(CameraComponent camera, TransformComponent xform)
        {
            var pos =  xform.Position + camera.PositionOffset;
            var orient = Quaternion.Normalize(xform.Orientation);

            var forward = Vector3.Transform(EngineGlobals.Forward, orient);
            var up = Vector3.Transform(EngineGlobals.Up, orient);

            camera.ViewMatrix = Matrix4x4.CreateLookAt(pos, pos + forward, up);
        }

        private void UpdateProjectionMatrix(CameraComponent camera)
        {
            // TODO move these to camera component
            var near1 = 0.1f;
            var far1 = 8000.0f;

            camera.ProjectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(camera.FieldOfView, camera.AspectRatio, near1, far1);
        }

        public static Matrix4x4 glmCreateLookAt(Vector3 cameraPosition, Vector3 cameraTarget, Vector3 cameraUpVector)
        {
            var f = Vector3.Normalize(cameraTarget - cameraPosition);
            var s = Vector3.Normalize(Vector3.Cross(f, cameraUpVector));
            var u = Vector3.Cross(s, f);

            Matrix4x4 result = Matrix4x4.Identity;

            result.M11 = s.X;
            result.M12 = s.Y;
            result.M13 = s.Z;

            result.M21 = u.X;
            result.M22 = u.Y;
            result.M23 = u.Z;

            result.M31 = -f.X;
            result.M32 = -f.Y;
            result.M33 = -f.Z;

            result.M41 = -Vector3.Dot(s, cameraPosition);
            result.M42 = -Vector3.Dot(u, cameraPosition);
            result.M43 = Vector3.Dot(f, cameraPosition);

            return result;
        }
    }
}
