using OpenH2.Core.Architecture;
using OpenH2.Foundation.Physics;
using System.Numerics;

namespace OpenH2.Engine.Components
{
    public class TransformComponent : Component, ITransform
    {
        public TransformComponent(Entity parent, Vector3 pos, Quaternion orientation) : base(parent)
        {
            this.Position = pos;
            this.Orientation = orientation;

            UpdateDerivedData();
        }

        public TransformComponent(Entity parent, Vector3 pos) : base(parent)
        {
            this.Position = pos;
            this.Orientation = Quaternion.Identity;

            UpdateDerivedData();
        }

        public TransformComponent(Entity parent, Quaternion orientation) : base(parent)
        {
            this.Position = Vector3.Zero;
            this.Orientation = orientation;

            UpdateDerivedData();
        }

        public Vector3 Position { get; set; } = Vector3.Zero;
        public Quaternion Orientation { get; set; } = Quaternion.Identity;
        public Vector3 Scale { get; set; } = Vector3.One;

        public Matrix4x4 TransformationMatrix { get; private set; }

        public void UpdateDerivedData()
        {
            Orientation = Quaternion.Normalize(Orientation);
            TransformationMatrix = CreateTransformationMatrix();
        }

        private Matrix4x4 CreateTransformationMatrix()
        {
            var translate = Matrix4x4.CreateTranslation(Position);
            var scale = Matrix4x4.CreateScale(Scale);

            Matrix4x4 rotate = Matrix4x4.CreateFromQuaternion(Orientation);

            var scaleRotate = Matrix4x4.Multiply(scale, rotate);

            return Matrix4x4.Multiply(scaleRotate, translate);
        }
    }
}
