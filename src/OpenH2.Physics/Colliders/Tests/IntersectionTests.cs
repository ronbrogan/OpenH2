using OpenH2.Physics.Colliders.Extensions;
using System;
using System.Numerics;

namespace OpenH2.Physics.Colliders.Tests
{
    /// <summary>
    /// Intersection tests, mostly ported from code from Millington 2010
    /// </summary>
    public static class IntersectionTests
    {
        public static bool BoxAndPlane(BoxCollider box, PlaneCollider plane)
        {
            var radius = box.TransformToAxis(plane.Normal);

            var distance = NumericsExtensions.DotWithAxis(plane.Normal, 3, box.Transform) - radius;

            return distance <= plane.Distance;
        }

        public static bool BoxAndBox(BoxCollider one,BoxCollider two)
        {
            // Find the vector between the two centers
            Vector3 toCenter = two.GetAxis(3) - one.GetAxis(3);

            return (
                // Check on box one's axes first
                OverlapOnAxis(one, two, one.GetAxis(0), toCenter) &&
                OverlapOnAxis(one, two, one.GetAxis(1), toCenter) &&
                OverlapOnAxis(one, two, one.GetAxis(2), toCenter) &&

                // And on two's
                OverlapOnAxis(one, two, two.GetAxis(0), toCenter) &&
                OverlapOnAxis(one, two, two.GetAxis(1), toCenter) &&
                OverlapOnAxis(one, two, two.GetAxis(2), toCenter) &&

                // Now on the cross products
                OverlapOnAxis(one, two, Vector3.Cross(one.GetAxis(0), two.GetAxis(0)), toCenter) &&
                OverlapOnAxis(one, two, Vector3.Cross(one.GetAxis(0), two.GetAxis(1)), toCenter) &&
                OverlapOnAxis(one, two, Vector3.Cross(one.GetAxis(0), two.GetAxis(2)), toCenter) &&
                OverlapOnAxis(one, two, Vector3.Cross(one.GetAxis(1), two.GetAxis(0)), toCenter) &&
                OverlapOnAxis(one, two, Vector3.Cross(one.GetAxis(1), two.GetAxis(1)), toCenter) &&
                OverlapOnAxis(one, two, Vector3.Cross(one.GetAxis(1), two.GetAxis(2)), toCenter) &&
                OverlapOnAxis(one, two, Vector3.Cross(one.GetAxis(2), two.GetAxis(0)), toCenter) &&
                OverlapOnAxis(one, two, Vector3.Cross(one.GetAxis(2), two.GetAxis(1)), toCenter) &&
                OverlapOnAxis(one, two, Vector3.Cross(one.GetAxis(2), two.GetAxis(2)), toCenter)
            );
        }

        private static bool OverlapOnAxis(BoxCollider one,BoxCollider two,Vector3 axis,Vector3 toCenter)
        {
            // Project the half-size of one onto axis
            float oneProject = BoxExtensions.TransformToAxis(one, axis);
            float twoProject = BoxExtensions.TransformToAxis(two, axis);

            // Project this onto the axis
            float distance = Math.Abs(Vector3.Dot(toCenter, axis));

            // Check for overlap
            return (distance<oneProject + twoProject);
        }
    }
}
