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
        public Matrix4x4 SunLightMatrix0;

        [FieldOffset(192)]
        public Matrix4x4 SunLightMatrix1;

        [FieldOffset(256)]
        public Matrix4x4 SunLightMatrix2;

        [FieldOffset(320)]
        public Matrix4x4 SunLightMatrix3;

        [FieldOffset(384)]
        public System.Numerics.Vector4 SunLightDistances;

        [FieldOffset(400)]
        public System.Numerics.Vector3 SunLightDirection;

        [FieldOffset(412)]
        private float pad;

        [FieldOffset(416)]
        public System.Numerics.Vector3 ViewPosition;

        [FieldOffset(428)]
        private float pad2;

        public static readonly int Size = Marshal.SizeOf<GlobalUniform>();

    }
}
