using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace OpenH2.Physics.Colliders.Extensions
{
    public static class BoxExtensions
    {
        public static float TransformToAxis(this BoxCollider box, Vector3 axis)
        {
            return box.HalfWidths.X * Math.Abs(NumericsExtensions.DotWithAxis(axis, 0, box.Transform))
                + box.HalfWidths.Y * Math.Abs(NumericsExtensions.DotWithAxis(axis, 1, box.Transform))
                + box.HalfWidths.Z * Math.Abs(NumericsExtensions.DotWithAxis(axis, 2, box.Transform));
        }

    }
}
