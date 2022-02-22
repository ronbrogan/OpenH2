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
        public Matrix4x4 SunLightMatrix;

        [FieldOffset(192)]
        public System.Numerics.Vector3 SunLightDirection;

        [FieldOffset(204)]
        private float pad;

        [FieldOffset(208)]
        public System.Numerics.Vector3 ViewPosition;

        [FieldOffset(220)]
        private float pad2;

        public static readonly int Size = Marshal.SizeOf<GlobalUniform>();

    }
}
