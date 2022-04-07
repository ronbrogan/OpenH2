using System.Runtime.InteropServices;

namespace OpenH2.Rendering.Shaders.ShadowMapping
{
    // Be careful moving/adding/removing properties
    // Sequential layout ensures that the offset of a field occurs at a multiple of the size
    // For example, float then long would be float@0, then 4 bytes of padding, then long@8
    // Thus the struct size would be 16 instead of the expected 12

    // This also has to match the std140 layout rules

    [StructLayout(LayoutKind.Sequential)]
    public struct ShadowMappingUniform
    {
        public static readonly int Size = Marshal.SizeOf<ShadowMappingUniform>();
    }
}
