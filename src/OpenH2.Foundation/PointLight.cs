using System.Numerics;

namespace OpenH2.Foundation
{
    public class PointLight
    {
        public Vector3 Position { get; set; } = Vector3.Zero;

        public Vector3 Color { get; set; } = Vector3.One;

        public float Radius { get; set; } = 10f;
    }
}
