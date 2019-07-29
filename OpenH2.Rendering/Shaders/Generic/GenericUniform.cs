using OpenTK;
using System;
using System.Numerics;
using System.Runtime.InteropServices;
using Vector4 = System.Numerics.Vector4;

namespace OpenH2.Rendering.Shaders.Generic
{
    // Be careful moving/adding/removing properties
    // Sequential layout pads to the size of the current field
    // For example, float then long would be float@0, then 4 bytes of padding, then long@8
    // Thus the struct size would be 16 instead of the expected 12

    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct GenericUniform
    {
        public Matrix4x4 ModelMatrix;
        public Matrix4x4 NormalMatrix;

        public bool UseDiffuseMap;
        public float DiffuseAmount;
        public long DiffuseMap;
        public Vector4 DiffuseColor;

        public bool UseSpecularMap;
        public float SpecularAmount;
        public long SpecularMap;
        public Vector4 SpecularColor;

        public bool UseNormalMap;
        public float NormalMapAmount;
        public long NormalMap;

        public bool UseEmissiveMap;
        public float EmissiveMapAmount;
        public long EmissiveMap;

        public bool UseDetailMap0;
        public float DetailMapAmount0;
        public long DetailMap0;

        public bool UseDetailMap1;
        public float DetailMapAmount1;
        public long DetailMap1;

        public static readonly int Size = BlittableValueType<GenericUniform>.Stride;
    }
}
