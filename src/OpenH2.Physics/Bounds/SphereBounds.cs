using OpenH2.Foundation.Physics;
using System.Numerics;

namespace OpenH2.Physics.Bounds
{
    public class SphereBounds : ISweepableBounds
    {
        private ITransform transform { get; set; }
        public Vector3 Center { get; set; }
        public float Radius { get; set; }

        public Vector3 Max() => transform.Position + Center + new Vector3(Radius);
        public Vector3 Min() => transform.Position + Center - new Vector3(Radius);

        public SphereBounds(ITransform xform, Vector3 center, float radius)
        {
            this.Center = center;
            this.Radius = radius;
            this.transform = xform;
        }
    }
}
