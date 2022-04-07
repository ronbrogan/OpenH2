
using System.Numerics;
using OpenH2.Foundation.Physics;

namespace OpenH2.Foundation
{
    public class CompositeTransform : ITransform
    {
        private readonly ITransform a;
        private readonly ITransform b;

        public CompositeTransform(ITransform a, ITransform b)
        {
            this.a = a;
            this.b = b;
        }

        public Vector3 Scale { get => a.Scale * b.Scale; set { a.Scale = value; b.Scale = Vector3.Zero; } }
        public Vector3 Position { get => a.Position + b.Position; set { a.Position = value; b.Position = Vector3.Zero; } }
        public Quaternion Orientation { get => Quaternion.Multiply(a.Orientation, b.Orientation); set { a.Orientation = value; b.Orientation = Quaternion.Identity; } }

        public Matrix4x4 TransformationMatrix => Matrix4x4.Multiply(a.TransformationMatrix, b.TransformationMatrix);

        public void UpdateDerivedData()
        {
            a.UpdateDerivedData();
            b.UpdateDerivedData();
        }

        public void UseTransformationMatrix(Matrix4x4 mat)
        {
            a.UseTransformationMatrix(mat);
            b.UseTransformationMatrix(Matrix4x4.Identity);
        }
    }


    public class Transform : ITransform
    {
        public Vector3 Scale { get; set; }
        public Vector3 Position { get; set; }
        public Quaternion Orientation { get; set; }

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

            var result = Matrix4x4.Multiply(scaleRotate, translate);

            return result;
        }

        public void UseTransformationMatrix(Matrix4x4 mat)
        {
            this.TransformationMatrix = mat;
            Matrix4x4.Decompose(mat, out var s, out var r, out var p);
            this.Scale = s;
            this.Orientation = r;
            this.Position = p;
        }
    }
}
