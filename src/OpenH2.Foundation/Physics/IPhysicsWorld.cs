using System.Numerics;

namespace OpenH2.Foundation.Physics
{
    public interface IPhysicsWorld
    {
        Vector3 Gravity { get; set; }
    }
}
