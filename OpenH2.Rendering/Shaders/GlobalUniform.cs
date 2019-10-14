using OpenTK;
using System.Numerics;
using System.Runtime.InteropServices;

namespace OpenH2.Rendering.Shaders
{
    [StructLayout(LayoutKind.Explicit)]
    public struct GlobalUniform
    {
        [FieldOffset(0)]
        public Matrix4x4 ViewMatrix;

        [FieldOffset(64)]
        public Matrix4x4 ProjectionMatrix;

        [FieldOffset(128)]
        public System.Numerics.Vector3 ViewPosition;

        public static readonly int Size = BlittableValueType<GlobalUniform>.Stride;

    }
}
