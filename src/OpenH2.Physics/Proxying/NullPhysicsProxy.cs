using System;
using System.Numerics;

namespace OpenH2.Physics.Proxying
{
    public class NullPhysicsProxy : IPhysicsProxy
    {
        public static IPhysicsProxy Instance = new NullPhysicsProxy();

        public Vector3 GetAngularVelocity() => Vector3.Zero;
        public Vector3 GetVelocity() => Vector3.Zero;

        public void AddForce(Vector3 force) { }
        public void AddForceAtLocalPoint(Vector3 force, Vector3 point) { }
        public void AddForceAtPoint(Vector3 force, Vector3 point) { }
        public void ApplyImpulse(Vector3 force) { }
        public void AddVelocity(Vector3 force) { }
        public void Move(Vector3 delta, double timestep) { }

        public void UseTransformationMatrix(Matrix4x4 transform) { }

        public RaycastHit[] Raycast(Vector3 direction, float maxDistance, int maxResults) => Array.Empty<RaycastHit>();
    }
}
