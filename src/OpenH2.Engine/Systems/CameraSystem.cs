using OpenH2.Core.Architecture;
using OpenH2.Engine.Components;
using OpenH2.Rendering.Abstractions;
using System.Numerics;

namespace OpenH2.Engine.Systems
{
    public class CameraSystem : WorldSystem
    {
        private int currentCameraMoveTicksRemaining = 0;
        private readonly IGraphicsHost graphics;

        public CameraSystem(World world, IGraphicsHost graphics) : base(world)
        {
            this.graphics = graphics;
        }

        public override void Update(double timestep)
        {
            if(currentCameraMoveTicksRemaining > 0)
            {
                currentCameraMoveTicksRemaining--;
            }
            
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

                if(camera.Dirty)
                {
                    UpdateProjectionMatrix(camera);
                }
            }
        }

        public void PerformCameraMove(int tickDuration)
        {
            this.currentCameraMoveTicksRemaining = tickDuration;
        }

        public int GetCameraMoveRemaining()
        {
            return currentCameraMoveTicksRemaining;
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
    }
}
