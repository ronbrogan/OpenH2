using OpenH2.Core.Tags;
using OpenH2.Foundation;
using System.Numerics;
using System.Runtime.InteropServices;

namespace OpenH2.Rendering.Shaders.Pointviz
{
    [StructLayout(LayoutKind.Sequential)]
    public struct PointvizUniform
    {
        public PointvizUniform(IMaterial<BitmapTag> material, MaterialBindings bindings, Matrix4x4 transform, Matrix4x4 inverted)
        {
            ModelMatrix = transform;
            NormalMatrix = Matrix4x4.Transpose(inverted);
            DiffuseColor = material.DiffuseColor;
            AlphaAmount = 1f;
        }

        public Matrix4x4 ModelMatrix;
        public Matrix4x4 NormalMatrix;

        public Vector4 DiffuseColor;
        public float AlphaAmount;

        public static readonly int Size = Marshal.SizeOf<PointvizUniform>();

    }
}
