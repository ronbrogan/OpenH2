using OpenTK;
using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Vector4 = System.Numerics.Vector4;

namespace OpenH2.Rendering.Shaders.Generic
{
    // Be careful moving/adding/removing properties
    // Sequential layout ensures that the offset of a field occurs at a multiple of the size
    // For example, float then long would be float@0, then 4 bytes of padding, then long@8
    // Thus the struct size would be 16 instead of the expected 12

    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct GenericUniform
    {
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
        public float DetailMap1Scale;
        public long DetailMap1Handle;

        public bool UseDetailMap2;
        public float DetailMap2Scale;
        public long DetailMap2Handle;

        public static readonly int Size = BlittableValueType<GenericUniform>.Stride;
    }
}
