using OpenH2.Foundation.Physics;
using System.Numerics;

namespace OpenH2.Physics.Bounds
{
    public class AxisAlignedBoundingBox : ISweepableBounds
    {
        public Vector3 Least { get; set; }
        public Vector3 Most { get; set; }

        public Vector3 Max() => Most;
        public Vector3 Min() => Least;
    }
}
