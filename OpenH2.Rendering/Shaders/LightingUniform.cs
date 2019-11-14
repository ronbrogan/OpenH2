using OpenH2.Foundation;
using OpenH2.Rendering.Abstractions;
using System.Numerics;
using System.Runtime.InteropServices;

namespace OpenH2.Rendering.Shaders
{
    [StructLayout(LayoutKind.Explicit)]
    public struct LightingUniform : ISized
    {
        [FieldOffset(0)]
        public PointLightUniform[] PointLights;

        public static readonly int Size = OpenTK.BlittableValueType<LightingUniform>.Stride;

        public int SizeOf()
        {
            var size = Size;

            size += PointLightUniform.Size * PointLights.Length;

            return size;
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct PointLightUniform
    {
        public PointLightUniform(PointLight light)
        {
            Position = new Vector4(light.Position, 0f);
            ColorAndRange = new Vector4(light.Color, light.Radius);
        }

        [FieldOffset(0)]
        public Vector4 Position;

        [FieldOffset(16)]
        public Vector4 ColorAndRange;

        public static readonly int Size = OpenTK.BlittableValueType<PointLightUniform>.Stride;
    }
}
