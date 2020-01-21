using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace OpenH2.Physics.Colliders.Tests
{
    public static class IntersectionTests
    {
        public static bool BoxAndPlane(BoxCollider box, PlaneCollider plane)
        {
            var radius = TransformToAxis(box, plane.Normal);

            var distance = DotWithAxis(plane.Normal, 3, box.Transform) - radius;

            return distance <= plane.Distance;
        }

        private static float TransformToAxis(BoxCollider box, Vector3 axis)
        {
            return box.HalfWidths.X * Math.Abs(DotWithAxis(axis, 0, box.Transform))
                + box.HalfWidths.Y * Math.Abs(DotWithAxis(axis, 1, box.Transform))
                + box.HalfWidths.Z * Math.Abs(DotWithAxis(axis, 2, box.Transform));
        }

        private static float DotWithAxis(Vector3 axis, int index, Matrix4x4 mat)
        {
            switch(index)
            {
                case 0:
                    return Vector3.Dot(axis, new Vector3(mat.M11, mat.M21, mat.M31));
                case 1:
                    return Vector3.Dot(axis, new Vector3(mat.M12, mat.M22, mat.M32));
                case 2:
                    return Vector3.Dot(axis, new Vector3(mat.M13, mat.M23, mat.M33));
                case 3:
                    return Vector3.Dot(axis, new Vector3(mat.M14, mat.M24, mat.M34));
                default:
                    throw new Exception("Bad axis provided");

            }
        }
    }
}
