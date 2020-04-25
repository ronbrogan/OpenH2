using OpenH2.Core.Tags;
using OpenH2.Foundation;
using System.Runtime.InteropServices;
using Vector4 = System.Numerics.Vector4;

namespace OpenH2.Rendering.Shaders.Skybox
{
    // Be careful moving/adding/removing properties
    // Sequential layout ensures that the offset of a field occurs at a multiple of the size
    // For example, float then long would be float@0, then 4 bytes of padding, then long@8
    // Thus the struct size would be 16 instead of the expected 12

    // This also has to match the std140 layout rules

    [StructLayout(LayoutKind.Sequential)]
    public struct SkyboxUniform
    {
        public SkyboxUniform(IMaterial<BitmapTag> material, MaterialBindings bindings)
        {
            DiffuseColor = material.DiffuseColor;
            UseDiffuse = bindings.DiffuseHandle != default;
            DiffuseHandle = bindings.DiffuseHandle;
            DiffuseAmount = 1f;
        }

        public bool UseDiffuse;
        public float DiffuseAmount;
        public long DiffuseHandle;
        public Vector4 DiffuseColor;

        public static readonly int Size = Marshal.SizeOf<SkyboxUniform>();
    }
}
