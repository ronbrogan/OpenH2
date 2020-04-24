using System.Numerics;

namespace OpenH2.Foundation.Physics
{
    public interface IRigidBody : IBody
    {        
        bool IsDynamic { get; }
        float Mass { get; }
        float InverseMass { get; }
        Vector3 CenterOfMass { get; }
        Matrix4x4 InertiaTensor { get; }

        Vector3 Velocity { get; }
        Vector3 AngularVelocity { get; }
    }
}
