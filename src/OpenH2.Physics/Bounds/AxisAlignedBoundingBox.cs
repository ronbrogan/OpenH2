using OpenH2.Foundation.Physics;
using System;
using System.Numerics;

namespace OpenH2.Physics.Bounds
{
    public class AxisAlignedBoundingBox : ISweepableBounds
    {
        public Vector3 Least { get; private set; }
        public Vector3 Most { get; private set; }

        public Vector3 Max => Most;
        public Vector3 Min => Least;
        public Vector3 Center { get; private set; }
        public float Radius { get; private set; }


        public AxisAlignedBoundingBox(Vector3 least, Vector3 most)
        {
            this.Least = least;
            this.Most = most;

            var halfWidths = (most - least) / 2;

            this.Center = least + halfWidths;
            this.Radius = Math.Max(halfWidths.X, Math.Max(halfWidths.Y, halfWidths.Z));
        }
    }
}
