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

        public static Quaternion FromH2vOrientation(Vector3 orient)
        {
            /*
             * This method is mathematically equivalent to these operations
             *
             * var q1 = Quaternion.CreateFromAxisAngle(new Vector3(0, 0, 1), orient.X);
             * var q2 = Quaternion.CreateFromAxisAngle(new Vector3(0, -1, 0), orient.Y);
             * var q3 = Quaternion.CreateFromAxisAngle(new Vector3(1, 0, 0), orient.Z);
             * 
             * return Quaternion.Normalize(q3 * q2 * q1);
             */

            float halfAngleX = orient.X * 0.5f;
            float halfAngleY = orient.Y * 0.5f;
            float halfAngleZ = orient.Z * 0.5f;

            float xs = (float)Math.Sin(halfAngleX);
            float ys = (float)Math.Sin(halfAngleY);
            float zs = (float)Math.Sin(halfAngleZ);

            float xc = (float)Math.Cos(halfAngleX);
            float yc = (float)Math.Cos(halfAngleY);
            float zc = (float)Math.Cos(halfAngleZ);

            float q1x = zs * yc;
            float q1y = -ys * zc;
            float q1z = zs * -ys;
            float q1w = zc * yc;

            Quaternion t2 = new Quaternion(
                q1x * xc + q1y * xs,
                q1y * xc - q1x * xs,
                q1z * xc + xs * q1w,
                q1w * xc - q1z * xs
            );

            return Quaternion.Normalize(t2);
        }
    }
}
