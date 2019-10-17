using OpenH2.Core.Tags;
using OpenH2.Foundation;
using OpenTK;
using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Vector4 = System.Numerics.Vector4;

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
        public GenericUniform(IMaterial<BitmapTag> material, Matrix4x4 transform, Matrix4x4 inverted)
        {
            ModelMatrix = transform;
            NormalMatrix = Matrix4x4.Transpose(inverted);
            DiffuseColor = new Vector4(material.DiffuseColor, 1);
            UseDiffuse = material.DiffuseHandle != default;
            DiffuseHandle = material.DiffuseHandle;
            DiffuseAmount = 1f;

            UseDetailMap1 = material.Detail1Handle != default;
            DetailMap1Amount = 1f;
            DetailMap1Handle = material.Detail1Handle;
            DetailMap1Scale = material.Detail1Scale;

            UseDetailMap2 = material.Detail2Handle != default;
            DetailMap2Amount = 1f;
            DetailMap2Handle = material.Detail2Handle;
            DetailMap2Scale = material.Detail2Scale;

            AlphaHandle = material.AlphaHandle;
            UseAlpha = material.AlphaHandle != default;
            AlphaAmount = 1f;

            // Currently unused
            UseSpecular = false;
            SpecularAmount = 0f;
            SpecularColor = Vector4.Zero;
            SpecularHandle = 0;
            UseNormalMap = false;
            NormalMapAmount = 0f;
            NormalMap = 0;
            UseEmissiveMap = false;
            EmissiveMapAmount = 0f;
            EmissiveMap = 0;
        }

        public Matrix4x4 ModelMatrix;
        public Matrix4x4 NormalMatrix;

        public bool UseDiffuse;
        public float DiffuseAmount;
        public long DiffuseHandle;
        public Vector4 DiffuseColor;

        public bool UseAlpha;
        public float AlphaAmount;
        public long AlphaHandle;

        public bool UseSpecular;
        public float SpecularAmount;
        public long SpecularHandle;
        public Vector4 SpecularColor;

        public bool UseNormalMap;
        public float NormalMapAmount;
        public long NormalMap;

        public bool UseEmissiveMap;
        public float EmissiveMapAmount;
        public long EmissiveMap;

        public bool UseDetailMap1;
        public float DetailMap1Amount;
        public long DetailMap1Handle;
        public Vector4 DetailMap1Scale;

        public bool UseDetailMap2;
        public float DetailMap2Amount;
        public long DetailMap2Handle;
        public Vector4 DetailMap2Scale;

        public static readonly int Size = BlittableValueType<GenericUniform>.Stride;
    }
}
