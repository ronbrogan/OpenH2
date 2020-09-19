using System.Numerics;

namespace OpenH2.Core.GameObjects
{
    public interface ILocationFlag : IGameObject
    {
        public Vector3 Position { get; }
    }
}
