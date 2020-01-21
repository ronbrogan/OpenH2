using System.Numerics;

namespace OpenH2.Foundation.Extensions
{
    public static class IntegrationExtensions
    {
        public static Matrix4x4 SetSkewSymmetric(this Matrix4x4 matrix, Vector3 vector)
        {
            matrix.M11 = matrix.M22 = matrix.M33 = 0;
            matrix.M12 = -vector.Z;
            matrix.M13 = vector.Y;
            matrix.M21 = vector.Z;
            matrix.M23 = -vector.X;
            matrix.M31 = -vector.Y;
            matrix.M32 = vector.X;

            return matrix;
        }

        public static Quaternion ApplyScaledVector(this Quaternion quat, Vector3 vector, float scale)
        {
            var q = new Quaternion(
                vector.X * scale,
                vector.Y * scale,
                vector.Z * scale,
                0);
            q *= quat;
            quat.W += q.W * 0.5f;
            quat.X += q.X * 0.5f;
            quat.Y += q.Y * 0.5f;
            quat.Z += q.Z * 0.5f;

            return quat;
        }

        public static float ScalarProduct(this Vector3 vector, Vector3 other)
        {
            return vector.X * other.X + vector.Y * other.Y + vector.Z * other.Z;
        }

        public static Matrix4x4 SetComponents(this Matrix4x4 matrix, Vector3 compOne, Vector3 compTwo, Vector3 compThree)
        {
            matrix.M11 = compOne.X;
            matrix.M21 = compOne.Y;
            matrix.M31 = compOne.Z;
            matrix.M12 = compTwo.X;
            matrix.M22 = compTwo.Y;
            matrix.M32 = compTwo.Z;
            matrix.M13 = compThree.X;
            matrix.M23 = compThree.Y;
            matrix.M33 = compThree.Z;

            return matrix;
        }
    }
}
