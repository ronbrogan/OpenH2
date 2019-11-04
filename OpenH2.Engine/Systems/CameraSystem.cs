using OpenH2.Core.Architecture;
using OpenH2.Engine.Components;
using System.Numerics;

namespace OpenH2.Engine.Systems
{
    public class CameraSystem : WorldSystem
    {
        public CameraSystem(World world) : base(world)
        {
        }

        public override void Update(double timestep)
        {
            var cameras = this.world.Components<CameraComponent>();

            foreach(var camera in cameras)
            {
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

        private void UpdateViewMatrix(CameraComponent camera, TransformComponent xform)
        {
            var pos =  xform.Position + camera.PositionOffset;
            var orient = Quaternion.Normalize(xform.Orientation);

            var rotation = Matrix4x4.CreateFromQuaternion(orient);
            var translation = Matrix4x4.CreateTranslation(-pos);

            camera.ViewMatrix = Matrix4x4.Multiply(translation, rotation);
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
