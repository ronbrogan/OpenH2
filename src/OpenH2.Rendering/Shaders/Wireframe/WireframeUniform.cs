using OpenH2.Core.Tags;
using OpenH2.Foundation;
using System.Numerics;
using System.Runtime.InteropServices;

namespace OpenH2.Rendering.Shaders.Wireframe
{
    [StructLayout(LayoutKind.Sequential)]
    public struct WireframeUniform
    {
        public WireframeUniform(IMaterial<BitmapTag> material)
        {
            DiffuseColor = material.DiffuseColor;
            AlphaAmount = 1f;
        }

        public Vector4 DiffuseColor;
        public float AlphaAmount;

        public static readonly int Size = Marshal.SizeOf<WireframeUniform>();

    }
}
