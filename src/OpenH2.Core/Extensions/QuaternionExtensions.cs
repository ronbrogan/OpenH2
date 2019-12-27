using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace OpenH2.Core.Extensions
{
    public static class QuaternionExtensions
    {
        public static Quaternion From3x3Mat(float[] vals)
        {
            var mat4 = new Matrix4x4(
                vals[0],
                vals[1],
                vals[2],
                0f,
                vals[3],
                vals[4],
                vals[5],
                0f,
                vals[6],
                vals[7],
                vals[8],
                0f,
                0f,
                0f,
                0f,
                1f);

            if (Matrix4x4.Decompose(mat4, out var _, out var q, out var _) == false)
            {
                return Quaternion.Identity;
            }

            return q;
        }
    }
}
