using System.Collections.Generic;
using System.Numerics;

namespace OpenH2.Foundation
{
    public class Model
    {
        public Mesh[] Meshes { get; set; }

        public Vector3 Position { get; set; } = Vector3.Zero;

        public Vector3 Orientation { get; set; } = Vector3.Zero;

        public Matrix4x4 CreateTransformationMatrix()
        {
            var translate = Matrix4x4.CreateTranslation(Position);
            var rotate = Matrix4x4.CreateFromYawPitchRoll(Orientation.Z, Orientation.X, Orientation.Y);

            return Matrix4x4.Multiply(translate, rotate);
        }
    }
}
