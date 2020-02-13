using System;
using System.Numerics;

namespace OpenH2.Physics.Colliders.Extensions
{
    public static class NumericsExtensions
    {
        public static float Get(this Vector3 vec, int axis)
        {
            switch (axis)
            {
                case 0:
                    return vec.X;
                case 1:
                    return vec.Y;
                case 2:
                    return vec.Z;
                default:
                    throw new Exception("Bad axis provided");

            }
        }

        public static Vector3 Set(this Vector3 vec, int axis, float val)
        {
            switch (axis)
            {
                case 0:
                    vec.X = val;
                    return vec;
                case 1:
                    vec.Y = val;
                    return vec;
                case 2:
                    vec.Z = val;
                    return vec;
                default:
                    throw new Exception("Bad axis provided");
            }
        }

        public static float DotWithAxis(Vector3 axis, int index, Matrix4x4 mat)
        {
            switch (index)
            {
                case 0:
                    return Vector3.Dot(axis, new Vector3(mat.M11, mat.M12, mat.M13));
                case 1:
                    return Vector3.Dot(axis, new Vector3(mat.M21, mat.M22, mat.M23));
                case 2:
                    return Vector3.Dot(axis, new Vector3(mat.M31, mat.M32, mat.M33));
                case 3:
                    return Vector3.Dot(axis, new Vector3(mat.M41, mat.M42, mat.M43));
                default:
                    throw new Exception("Bad axis provided");

            }
        }

        // If useOne is true, and the contact point is outside
        // the edge (in the case of an edge-face contact) then
        // we use one's midpoint, otherwise we use two's.
        public static Vector3 ContactPoint(Vector3 pOne, Vector3 dOne, float oneSize, Vector3 pTwo, Vector3 dTwo, float twoSize, bool useOne)
        {
            Vector3 toSt, cOne, cTwo;
            float dpStaOne, dpStaTwo, dpOneTwo, smOne, smTwo;
            float denom, mua, mub;

            smOne = dOne.LengthSquared();
            smTwo = dTwo.LengthSquared();
            dpOneTwo = Vector3.Dot(dTwo, dOne);

            toSt = pOne - pTwo;
            dpStaOne = Vector3.Dot(dOne, toSt);
            dpStaTwo = Vector3.Dot(dTwo, toSt);

            denom = smOne * smTwo - dpOneTwo * dpOneTwo;

            // Zero denominator indicates parrallel lines
            if (Math.Abs(denom) < 0.0001f)
            {
                return useOne ? pOne : pTwo;
            }

            mua = (dpOneTwo * dpStaTwo - smTwo * dpStaOne) / denom;
            mub = (smOne * dpStaTwo - dpOneTwo * dpStaOne) / denom;

            // If either of the edges has the nearest point out
            // of bounds, then the edges aren't crossed, we have
            // an edge-face contact. Our point is on the edge, which
            // we know from the useOne parameter.
            if (mua > oneSize ||
                mua < -oneSize ||
                mub > twoSize ||
                mub < -twoSize)
            {
                return useOne ? pOne : pTwo;
            }
            else
            {
                cOne = pOne + dOne * mua;
                cTwo = pTwo + dTwo * mub;

                return cOne * 0.5f + cTwo * 0.5f;
            }
        }
    }
}
