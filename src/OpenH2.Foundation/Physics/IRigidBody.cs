using System.Numerics;

namespace OpenH2.Foundation.Physics
{
    public interface IRigidBody : IBody
    {
        Vector3 ForceAccumulator { get; set; }
        Vector3 TorqueAccumulator { get; set; }

        Vector3 Acceleration { get; set; }
        Vector3 PreviousAcceleration { get; set; }

        Vector3 Velocity { get; set; }
        Vector3 AngularVelocity { get; set; }

        float Mass { get; set; }
        float InverseMass { get; }
        Matrix4x4 InverseInertiaBody { get; set; }
        Matrix4x4 InverseInertiaWorld { get; }

        void UpdateDerivedData();

        void AddVelocity(Vector3 deltaV);
        void AddRotation(Vector3 deltaAngularV);
        void AddForce(Vector3 force);
        void AddForceAtPoint(Vector3 force, Vector3 point);
        void AddTorque(Vector3 torque);

        void ResetAccumulators();
    }
}
