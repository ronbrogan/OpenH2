using OpenH2.Physics.Proxying;
using PhysX;
using System;
using System.Linq;
using System.Numerics;

namespace OpenH2.Physx.Proxies
{
    public class KinematicCharacterControllerProxy : IPhysicsProxy
    {
        public Controller Controller { get; }

        public KinematicCharacterControllerProxy(Controller ctrl)
        {
            this.Controller = ctrl;
        }

        public Vector3 GetAngularVelocity() => this.Controller.Actor.AngularVelocity;
        public Vector3 GetVelocity() => this.Controller.Actor.LinearVelocity;

        public void AddForce(Vector3 force) { }
        public void AddForceAtLocalPoint(Vector3 force, Vector3 point) { }
        public void AddForceAtPoint(Vector3 force, Vector3 point) { }
        public void ApplyImpulse(Vector3 force) { }
        public void AddVelocity(Vector3 force) { }
        public void Move(Vector3 delta, double timestep) => this.Controller.Move(delta, TimeSpan.FromSeconds(timestep));

        public void UseTransformationMatrix(Matrix4x4 transform) => this.Controller.Position = transform.Translation;

        public Physics.Core.RaycastHit[] Raycast(Vector3 direction, float maxDistance, int maxResults)
        {
            var hits = this.Controller.Actor.Scene.Raycast(this.Controller.Position, direction, maxDistance, maxResults);

            return hits.Select(h => new Physics.Core.RaycastHit(
                h.Distance,
                h.Position,
                h.Normal
            )).ToArray();
        }
    }
}
