using OpenH2.Foundation.Physics;
using System.Numerics;

namespace OpenH2.Physics.Bounds
{
    public class SphereBounds : ISweepableBounds
    {
        private ITransform transform { get; set; }
        private Vector3 centerOffset { get; set; }

        public Vector3 Max => transform.Position + centerOffset + new Vector3(Radius);
        public Vector3 Min => transform.Position + centerOffset - new Vector3(Radius);
        public Vector3 Center => transform.Position + centerOffset;
        public float Radius { get; set; }

        public SphereBounds(ITransform xform, Vector3 offset, float radius)
        {
            this.centerOffset = offset;
            this.Radius = radius;
            this.transform = xform;
        }
    }
}
