using OpenH2.Core.Architecture;
using System.Numerics;

namespace OpenH2.Engine.Components
{
    public class TransformComponent : Component
    {
        public TransformComponent(Entity parent) : base(parent)
        {
        }

        public Vector3 Position { get; set; } = Vector3.Zero;

        public Vector3 Scale { get; set; } = Vector3.One;

        public Quaternion Orientation { get; set; } = Quaternion.Identity;

        public Matrix4x4 CreateTransformationMatrix()
        {
            var translate = Matrix4x4.CreateTranslation(Position);
            var scale = Matrix4x4.CreateScale(Scale);

            Matrix4x4 rotate = Matrix4x4.CreateFromQuaternion(Orientation);

            var scaleRotate = Matrix4x4.Multiply(scale, rotate);

            return Matrix4x4.Multiply(scaleRotate, translate);
        }
    }
}
