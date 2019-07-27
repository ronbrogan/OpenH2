using OpenTK;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace OpenH2.Rendering.Shaders
{
    [StructLayout(LayoutKind.Explicit)]
    public struct GenericUniform : IEquatable<GenericUniform>
    {
        [FieldOffset(0)]
        public Matrix4x4 ModelMatrix;

        [FieldOffset(64)]
        public Matrix4x4 NormalMatrix;

        [FieldOffset(128)]
        public System.Numerics.Vector4 DiffuseColor;

        [FieldOffset(144)]
        public System.Numerics.Vector4 SpecularColor;

        public static readonly int Size = BlittableValueType<GenericUniform>.Stride;

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = ModelMatrix.GetHashCode();
                hashCode = (hashCode * 397) ^ NormalMatrix.GetHashCode();
                hashCode = (hashCode * 397) ^ DiffuseColor.GetHashCode();
                hashCode = (hashCode * 397) ^ SpecularColor.GetHashCode();
                return hashCode;
            }
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is GenericUniform && Equals((GenericUniform)obj);
        }

        public bool Equals(GenericUniform other)
        {
            return ModelMatrix == other.ModelMatrix &&
                NormalMatrix == other.NormalMatrix &&
                DiffuseColor == other.DiffuseColor &&
                SpecularColor == other.SpecularColor;
        }

        public static bool operator ==(GenericUniform g1, GenericUniform g2)
        {
            return g1.Equals(g2);
        }

        public static bool operator !=(GenericUniform g1, GenericUniform g2)
        {
            return !g1.Equals(g2);
        }
    }
}
