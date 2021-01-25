
using OpenH2.Foundation.Physics;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace OpenH2.Foundation
{
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
