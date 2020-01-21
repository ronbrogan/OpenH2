using OpenH2.Foundation.Extensions;
using OpenH2.Foundation.Physics;
using OpenH2.Physics.Abstractions;
using System;
using System.Numerics;

namespace OpenH2.Physics.Simulation
{
    public class RigidBodyIntegrator : IBodyIntegrator<IRigidBody>
    {
        private const float LinearDamping = 0.99f;
        private const float AngularDamping = 0.99f;

        // REF: Millington
        public void Integrate(IRigidBody body, float timestep)
        {
            if (!body.IsAwake) return;

            // Calculate linear acceleration from force inputs.
            body.PreviousAcceleration = body.Acceleration;
            body.PreviousAcceleration += Vector3.Multiply(body.ForceAccumulator, body.InverseMass);

            // Calculate angular acceleration from torque inputs.
            Vector3 angularAcceleration = Vector3.Transform(body.TorqueAccumulator, body.InverseInertiaWorld);

            // Adjust velocities
            // Update linear velocity from both acceleration and impulse.
            body.Velocity += Vector3.Multiply(body.PreviousAcceleration, timestep);

            // Update angular velocity from both acceleration and impulse.
            body.AngularVelocity += Vector3.Multiply(angularAcceleration, timestep);

            // Impose drag.
            body.Velocity = Vector3.Multiply(body.Velocity, (float)Math.Pow(LinearDamping, timestep));
            body.AngularVelocity = Vector3.Multiply(body.AngularVelocity, (float)Math.Pow(AngularDamping, timestep));

            // Adjust positions
            // Update linear position.
            body.Transform.Position += Vector3.Multiply(body.Velocity, timestep);

            // Update angular position.
            body.Transform.Orientation = body.Transform.Orientation.ApplyScaledVector(body.AngularVelocity, timestep);

            // Normalise the orientation, and update the matrices with the new
            // position and orientation
            body.UpdateDerivedData();

            // Clear accumulators.
            body.ResetAccumulators();

            // TODO: body sleep
            // Update the kinetic energy store, and possibly put the body to
            // sleep.
            //if (canSleep)
            //{
            //    real currentMotion = velocity.scalarProduct(velocity) +
            //        rotation.scalarProduct(rotation);

            //    real bias = real_pow(0.5, timestep);
            //    motion = bias * motion + (1 - bias) * currentMotion;

            //    if (motion < sleepEpsilon) setAwake(false);
            //    else if (motion > 10 * sleepEpsilon) motion = 10 * sleepEpsilon;
            //}
        }
    }
}
