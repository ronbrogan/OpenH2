using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace OpenH2.Core.Extensions
{
    public static class VectorExtensions
    {
        /// <summary>
        /// Get yaw component (Z) when using Z-up coordinates
        /// </summary>
        /// <param name="vec"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Yaw(this Vector3 vec)
        {
            return vec.Z;
        }

        /// <summary>
        /// Get pitch component (X) when using Z-up coordinates
        /// </summary>
        /// <param name="vec"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Pitch(this Vector3 vec)
        {
            return vec.X;
        }

        /// <summary>
        /// Get roll component (Y) when using Z-up coordinates
        /// </summary>
        /// <param name="vec"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Roll(this Vector3 vec)
        {
            return vec.Y;
        }

        public static Vector4 RandomColor(int shade = 128)
        {
            var mix = new Vector4(shade, shade, shade, 256);

            Random random = new Random();

            var rando = new Vector4(random.Next(256), random.Next(256), random.Next(256), 256);

            return (mix + rando) / 512;
        }

        public static Vector3 Random(int min, int max)
        {
            Random random = new Random();

            return new Vector3(random.Next(min, max), random.Next(min, max), random.Next(min, max));
        }

        public static Vector3 OrthoNormalize(Vector3 normal, Vector3 tangent)
        {
            normal = Vector3.Normalize(normal);

            var dot = Vector3.Dot(tangent, normal);

            var proj = Vector3.Multiply(normal, dot);

            tangent = Vector3.Subtract(tangent, proj);

            return Vector3.Normalize(tangent);
        }
    }
}
