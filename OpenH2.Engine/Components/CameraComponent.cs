using OpenH2.Core.Architecture;
using OpenH2.Core.Extensions;
using System;
using System.Numerics;

namespace OpenH2.Engine.Components
{
    public class CameraComponent : Component
    {
        public Vector3 PositionOffset { get; set; }
        public Vector3 OrientationOffset { get; set; }
        public float FieldOfView { get; set; }
        public float AspectRatio { get; private set; }
        public Matrix4x4 ProjectionMatrix { get; set; }

        public CameraComponent(Entity parent) : base(parent)
        {
            var piOn2 = (float)Math.PI / 2f;

            PositionOffset = Vector3.Zero;
            OrientationOffset = Vector3.Zero;// new Vector3(0, -piOn2, 0);
            FieldOfView = MathF.PI / 2;
            AspectRatio = 16f / 9f;

            this.UpdateProjectionMatrix();

            // TODO: figure out where this fits:
            // Clamp Pitch to +- 90deg
            //var clampedPitch = Math.Max(Math.Min(MathF.PI / 2, Orientation.Y), MathF.PI * -0.5);
        }

        // TODO: move to system
        public Matrix4x4 CalculateViewMatrix(Vector3 externalPos, Vector3 orientation)
        {
            var pos = (PositionOffset + externalPos);
            var or = orientation + OrientationOffset;

            var qPitch = Quaternion.CreateFromAxisAngle(new Vector3(1, 0, 0), or.Pitch());
            var qYaw = Quaternion.CreateFromAxisAngle(new Vector3(0, 0, 1), or.Yaw());

            //For a FPS camera we can omit roll
            var orient = Quaternion.Normalize(qPitch * qYaw);
            var rotation = Matrix4x4.CreateFromQuaternion(orient);
            var translation = Matrix4x4.CreateTranslation(pos);

            return Matrix4x4.Multiply(translation, rotation);
        }

        private void UpdateProjectionMatrix()
        {
            // TODO make these configurable on the fly
            var near1 = 0.1f;
            var far1 = 8000.0f;

            ProjectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(this.FieldOfView, this.AspectRatio, near1, far1);
        }
    }
}
