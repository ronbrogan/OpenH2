using System;
using System.Numerics;

namespace OpenH2.Foundation
{
    public class Model<TTexture>
    {
        public Mesh<TTexture>[] Meshes { get; set; }

        public Vector3 Position { get; set; } = Vector3.Zero;

        public Quaternion Orientation { get; set; } = Quaternion.Identity;

        public Vector3 Scale { get; set; } = Vector3.One;

        public string Note { get; set; }

        public ModelFlags Flags { get; set; }

        public Matrix4x4 CreateTransformationMatrix()
        {
            var translate = Matrix4x4.CreateTranslation(Position);
            var rotate = Matrix4x4.CreateFromQuaternion(Orientation);
            var scale = Matrix4x4.CreateScale(Scale);

            var scaleRotate = Matrix4x4.Multiply(scale, rotate);

            return Matrix4x4.Multiply(scaleRotate, translate);
        }
    }

    [Flags]
    public enum ModelFlags
    {
        Diffuse         = 1 << 0,
        CastsShadows    = 1 << 1,
        ReceivesShadows = 1 << 2,
        IsStatic        = 1 << 3,
        IsSkybox        = 1 << 4,
        IsTransparent   = 1 << 5,
        Wireframe       = 1 << 6,
        DebugViz        = 1 << 7
    }
}
