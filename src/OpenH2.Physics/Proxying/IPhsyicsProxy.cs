using OpenH2.Physics.Core;
using System.Numerics;

namespace OpenH2.Physics.Proxying
{
    public interface IPhysicsProxy
    {
        Vector3 GetVelocity();
        Vector3 GetAngularVelocity();


        void AddForce(Vector3 force);
        void AddForceAtPoint(Vector3 force, Vector3 point);
        void AddForceAtLocalPoint(Vector3 force, Vector3 point);
        void UseTransformationMatrix(Matrix4x4 transform);
        void ApplyImpulse(Vector3 force);
        void AddVelocity(Vector3 force);
        void Move(Vector3 delta, double timestep);

        RaycastHit[] Raycast(Vector3 direction, float maxDistance, int maxResults);
    }
}
