using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

namespace OpenH2.Foundation.Numerics
{
    public struct VectorD3 : IEquatable<VectorD3>
    {
        double X;
        double Y;
        double Z;

        public VectorD3(double x, double y, double z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        public VectorD3(Vector3 v)
        {
            this.X = v.X;
            this.Y = v.Y;
            this.Z = v.Z;
        }

        public Vector3 ToSingle()
        {
            return new Vector3((float)X, (float)Y, (float)Z);
        }

        public static VectorD3 Zero => new VectorD3(0, 0, 0);
        public static VectorD3 One => new VectorD3(1, 1, 1);


        /// <summary>
        /// Transforms a vector by the given matrix.
        /// </summary>
        /// <param name="position">The source vector.</param>
        /// <param name="matrix">The transformation matrix.</param>
        /// <returns>The transformed vector.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorD3 Transform(VectorD3 position, Matrix4x4 matrix)
        {
            return new VectorD3(
                position.X * matrix.M11 + position.Y * matrix.M21 + position.Z * matrix.M31 + matrix.M41,
                position.X * matrix.M12 + position.Y * matrix.M22 + position.Z * matrix.M32 + matrix.M42,
                position.X * matrix.M13 + position.Y * matrix.M23 + position.Z * matrix.M33 + matrix.M43);
        }

        /// <summary>
        /// Computes the cross product of two vectors.
        /// </summary>
        /// <param name="vector1">The first vector.</param>
        /// <param name="vector2">The second vector.</param>
        /// <returns>The cross product.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static VectorD3 Cross(VectorD3 vector1, VectorD3 vector2)
        {
            return new VectorD3(
                vector1.Y * vector2.Z - vector1.Z * vector2.Y,
                vector1.Z * vector2.X - vector1.X * vector2.Z,
                vector1.X * vector2.Y - vector1.Y * vector2.X);
        }

        /// <summary>
        /// Returns the dot product of two vectors.
        /// </summary>
        /// <param name="vector1">The first vector.</param>
        /// <param name="vector2">The second vector.</param>
        /// <returns>The dot product.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static double Dot(VectorD3 vector1, VectorD3 vector2)
        {
            return vector1.X * vector2.X +
                   vector1.Y * vector2.Y +
                   vector1.Z * vector2.Z;
        }

        /// <summary>
        /// Returns the hash code for this instance.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            int hash = this.X.GetHashCode();
            hash ^= this.Y.GetHashCode();
            hash ^= this.Z.GetHashCode();
            return hash;
        }

        /// <summary>
        /// Returns a boolean indicating whether the given Object is equal to this Vector3 instance.
        /// </summary>
        /// <param name="obj">The Object to compare against.</param>
        /// <returns>True if the Object is equal to this Vector3; False otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
        {
            if (!(obj is Vector3))
                return false;
            return Equals((Vector3)obj);
        }

        public bool Equals(VectorD3 other)
        {
            return this.X == other.X
                && this.Y == other.Y
                && this.Z == other.Z;
        }

        public static bool operator ==(VectorD3 v1, VectorD3 v2)
        {
            return v1.Equals(v2);
        }

        public static bool operator !=(VectorD3 v1, VectorD3 v2)
        {
            return !v1.Equals(v2);
        }

        public static VectorD3 operator -(VectorD3 v1, VectorD3 v2)
        {
            return new VectorD3(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z);
        }

        public static VectorD3 operator -(VectorD3 v)
        {
            return new VectorD3(-v.X, -v.Y, -v.Z);
        }

        public double this[int index]
        {
            get => index switch
            {
                0 => X,
                1 => Y,
                2 => Z
            };

            set
            {
                switch (index)
                {
                    case 0: X = value; break;
                    case 1: Y = value; break;
                    case 2: Z = value; break;
                    default:
                        break;
                }
            }
        }
    }
}
