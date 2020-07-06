using OpenH2.Physics.Proxying;
using PhysX;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;

namespace OpenH2.Physx.Proxies
{
    public class RigidBodyProxy : IPhysicsProxy
    {
        public RigidBody RigidBody { get; }

        public RigidBodyProxy(RigidBody body)
        {
            this.RigidBody = body;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3 GetVelocity() => this.RigidBody.LinearVelocity;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3 GetAngularVelocity() => this.RigidBody.AngularVelocity;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddForce(Vector3 force) => this.RigidBody.AddForce(force);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddForceAtPoint(Vector3 force, Vector3 point) => this.RigidBody.AddForceAtPosition(force, point);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddForceAtLocalPoint(Vector3 force, Vector3 point) => this.RigidBody.AddForceAtLocalPosition(force, point);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ApplyImpulse(Vector3 force) => this.RigidBody.AddForce(force, ForceMode.Impulse, true);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddVelocity(Vector3 force) => this.RigidBody.AddForce(force, ForceMode.VelocityChange, true);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Move(Vector3 delta, double timestep) => this.RigidBody.GlobalPosePosition += delta;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Physics.Core.RaycastHit[] Raycast(Vector3 direction, float maxDistance, int maxResults)
        {
            var hits = this.RigidBody.Scene.Raycast(this.RigidBody.GlobalPosePosition,
                direction,
                maxDistance,
                maxResults,
                HitFlag.Default | HitFlag.MeshMultiple);

            var result = new List<Physics.Core.RaycastHit>(hits.Length);

            for (var i = 0; i < hits.Length; i++)
            {
                var h = hits[i];

                // Skip self
                if (h.Actor == this.RigidBody)
                    continue;

                result.Add(new Physics.Core.RaycastHit
                (
                    h.Distance,
                    h.Flags.HasFlag(HitFlag.Position) ? h.Position : Vector3.Zero,
                    h.Flags.HasFlag(HitFlag.Normal) ? h.Normal : Vector3.Zero
                ));
            }

            return result.ToArray();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UseTransformationMatrix(Matrix4x4 transform) => this.RigidBody.GlobalPose = transform;
    }
}
