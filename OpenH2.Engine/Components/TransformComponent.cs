using OpenH2.Core.Architecture;
using System.Numerics;

namespace OpenH2.Engine.Components
{
    public class TransformComponent : Component
    {
        public TransformComponent(Entity parent) : base(parent)
        {
        }

        public Vector3 Position { get; set; }

        public Vector3 Orientation { get; set; }
    }
}
