using OpenH2.Core.Architecture;
using System;
using System.Numerics;

namespace OpenH2.Engine.Components
{
    public class CameraComponent : Component
    {
        private float fieldOfView;
        private float aspectRatio;

        public bool Dirty { get; set; }

        public Vector3 PositionOffset { get; set; }
        public Matrix4x4 ProjectionMatrix { get; set; }
        public Matrix4x4 ViewMatrix { get; set; }

        public float FieldOfView { get => fieldOfView; set { fieldOfView = value; this.Dirty = true; } }
        public float AspectRatio { get => aspectRatio; private set { aspectRatio = value; this.Dirty = true; } }
        

        public CameraComponent(Entity parent) : base(parent)
        {
            PositionOffset = Vector3.Zero;
            FieldOfView = MathF.PI / 2;
            AspectRatio = 16f / 9f;

            // TODO: figure out where this fits:
            // Clamp Pitch to +- 90deg
            //var clampedPitch = Math.Max(Math.Min(MathF.PI / 2, Orientation.Y), MathF.PI * -0.5);
        }
    }
}
