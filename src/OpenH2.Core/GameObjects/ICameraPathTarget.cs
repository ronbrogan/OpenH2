using System.Numerics;

namespace OpenH2.Core.GameObjects
{
    public interface ICameraPathTarget
    {
        public Vector3 Position { get; }

        public Vector3 Orientation { get; }

        public float FieldOfView { get; }
    }
}
