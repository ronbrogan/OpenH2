using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;

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

        public static Vector3 RandomColor(int shade = 128)
        {
            var mix = new Vector3(shade, shade, shade);

            Random random = new Random();

            var rando = new Vector3(random.Next(256), random.Next(256), random.Next(256));

            return (mix + rando) / 512;
        }
    }
}
