using System.Collections.Generic;
using System.Numerics;

namespace OpenH2.Foundation
{
    public class Model
    {
        public Mesh[] Meshes { get; set; }

        public Vector3 Position { get; set; } = Vector3.Zero;

        public Vector3 Orientation { get; set; } = Vector3.Zero;

        public Vector3 Scale { get; set; } = Vector3.One;

        public string Note { get; set; }

        public Matrix4x4 CreateTransformationMatrix()
        {
            var translate = Matrix4x4.CreateTranslation(Position);
            var rotate = Matrix4x4.CreateFromYawPitchRoll(Orientation.Z, Orientation.Y, Orientation.X);
            var scale = Matrix4x4.CreateScale(Scale);

            var scaleRotate = Matrix4x4.Multiply(scale, rotate);

            return Matrix4x4.Multiply(scaleRotate, translate);
        }
    }
}
