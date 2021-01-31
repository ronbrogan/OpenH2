using OpenH2.Core.Tags;
using OpenH2.Foundation;
using System.Numerics;
using System.Runtime.InteropServices;

namespace OpenH2.Rendering.Shaders.Generic
{
    // Be careful moving/adding/removing properties
    // Sequential layout ensures that the offset of a field occurs at a multiple of the size
    // For example; float then long would be float@0; then 4 bytes of padding; then long@8
    // Thus the struct size would be 16 instead of the expected 12

    // This also has to match the std140 layout rules

    [StructLayout(LayoutKind.Sequential)]
    public struct GenericUniform
    {
        public GenericUniform(Mesh<BitmapTag> mesh, Vector4 colorChangeData, MaterialBindings bindings)
        {
            var material = mesh.Material;

            DiffuseColor = material.DiffuseColor;
            UseDiffuse = bindings.DiffuseHandle != default;
            DiffuseHandle = bindings.DiffuseHandle;
            DiffuseAmount = 1f;

            UseDetailMap1 = bindings.Detail1Handle != default;
            DetailMap1Amount = 1f;
            DetailMap1Handle = bindings.Detail1Handle;
            DetailMap1Scale = material.Detail1Scale;

            UseDetailMap2 = bindings.Detail2Handle != default;
            DetailMap2Amount = 1f;
            DetailMap2Handle = bindings.Detail2Handle;
            DetailMap2Scale = material.Detail2Scale;

            AlphaHandle = bindings.AlphaHandle;
            UseAlpha = bindings.AlphaHandle != default;
            AlphaChannel = new Vector4(material.AlphaFromRed ? 1f : 0f, 0, 0, material.AlphaFromRed ? 0f : 1f);
            AlphaAmount = 1f;

            UseEmissiveMap = bindings.EmissiveHandle != default;
            EmissiveMap = bindings.EmissiveHandle;
            EmissiveType = (int)material.EmissiveType;
            EmissiveArguments = material.EmissiveArguments;

            UseNormalMap = bindings.NormalHandle != default;
            NormalMap = bindings.NormalHandle;
            NormalMapAmount = 1f;
            NormalMapScale = material.NormalMapScale;

            ChangeColor = bindings.ColorChangeHandle != default;
            ColorChangeMaskHandle = bindings.ColorChangeHandle;
            ColorChangeAmount = 1f;
            ColorChangeColor = colorChangeData;

            // Currently unused
            UseSpecular = false;
            SpecularAmount = 0f;
            SpecularColor = Vector4.Zero;
            SpecularHandle = 0;
        }

        public bool UseDiffuse;
        public float DiffuseAmount;
        public long DiffuseHandle;
        public Vector4 DiffuseColor;

        public bool UseAlpha;
        public float AlphaAmount;
        public long AlphaHandle;
        public Vector4 AlphaChannel;

        public bool UseSpecular;
        public float SpecularAmount;
        public long SpecularHandle;
        public Vector4 SpecularColor;

        public bool UseNormalMap;
        public float NormalMapAmount;
        public long NormalMap;
        public Vector4 NormalMapScale;

        public bool UseEmissiveMap;
        public int EmissiveType;
        public long EmissiveMap;
        public Vector4 EmissiveArguments;

        public bool UseDetailMap1;
        public float DetailMap1Amount;
        public long DetailMap1Handle;
        public Vector4 DetailMap1Scale;

        public bool UseDetailMap2;
        public float DetailMap2Amount;
        public long DetailMap2Handle;
        public Vector4 DetailMap2Scale;

        public bool ChangeColor;
        public float ColorChangeAmount;
        public long ColorChangeMaskHandle;
        public Vector4 ColorChangeColor;

        public static readonly int Size = Marshal.SizeOf<GenericUniform>();
    }
}
