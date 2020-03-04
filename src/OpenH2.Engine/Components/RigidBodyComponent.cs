using OpenH2.Core.Architecture;
using OpenH2.Foundation.Physics;
using System.Numerics;

namespace OpenH2.Engine.Components
{
    public class RigidBodyComponent : Component, IRigidBody
    {
        private static Vector3 AccelerationDueToGravity = new Vector3(0, 0, -9.8f);

        public Vector3 ForceAccumulator { get; set; }
        public Vector3 TorqueAccumulator { get; set; }

        public bool IsAwake { get; set; } = true;
        public bool IsStatic => false;
        public ICollider Collider { get; set; }
        public ITransform Transform { get; }

        // "Constant" attributes
        public Vector3 CenterOfMassOffset { get; set; }
        private float mass;
        private float inverseMass;
        public float Mass { get => mass; set { mass = value; inverseMass = 1 / value; } }
        public float InverseMass => inverseMass;
        public Matrix4x4 InverseInertiaBody { get; set; }
        public Matrix4x4 InverseInertiaWorld { get; private set; }

        // "Secondary" or "calculated" attributes
        public Vector3 Acceleration { get; set; }
        public Vector3 PreviousAcceleration { get; set; }
        public Vector3 Velocity { get; set; }
        public Vector3 AngularVelocity { get; set; }

        public RigidBodyComponent(Entity parent, 
            TransformComponent xform, 
            Matrix4x4 inertiaTensor,
            Vector3 centerOfMassOffset = default)
            : base(parent)
        {
            this.Transform = xform;
            this.Acceleration = AccelerationDueToGravity;
            this.CenterOfMassOffset = centerOfMassOffset;
            this.Mass = 1f;

            // Set non-rotational components of matrix to standard values to ensure inversion succeeds
            // TODO: investigate if these components of the matrix have any meaningful data
            inertiaTensor.M14 = 0;
            inertiaTensor.M24 = 0;
            inertiaTensor.M34 = 0;
            inertiaTensor.M41 = 0;
            inertiaTensor.M42 = 0;
            inertiaTensor.M43 = 0;
            inertiaTensor.M44 = 1;

            if (Matrix4x4.Invert(inertiaTensor, out var inv))
            {
                this.InverseInertiaBody = inv;
            }
        }

        public void UpdateDerivedData()
        {
            Transform.UpdateDerivedData();

            // Calculate the inertiaTensor in world space.
            var localWorld = InverseInertiaWorld;
            TransformInertiaTensor(InverseInertiaBody, Transform.TransformationMatrix, ref localWorld);
            InverseInertiaWorld = localWorld;
        }

        public void AddVelocity(Vector3 deltaV)
        {
            Velocity += deltaV;
        }

        public void AddRotation(Vector3 deltaAngularV)
        {
            AngularVelocity += deltaAngularV;
        }

        public void AddForce(Vector3 force)
        {
            ForceAccumulator += force;
            IsAwake = true;
        }

        public void AddForceAtPoint(Vector3 force, Vector3 point)
        {
            var pointRelativeToCenterOfMass = point - Transform.Position + CenterOfMassOffset;

            ForceAccumulator += force;
            TorqueAccumulator += Vector3.Cross(pointRelativeToCenterOfMass, force);

            IsAwake = true;
        }

        public void AddTorque(Vector3 torque)
        {
            TorqueAccumulator += torque;
            IsAwake = true;
        }

        public void ResetAccumulators()
        {
            ForceAccumulator = Vector3.Zero;
            TorqueAccumulator = Vector3.Zero;
        }

        /// <summary>
        /// Transform the Body inverse inertia tensor into world space by only using the rotational 
        /// component of the provided transformation matrix
        /// </summary>
        private void TransformInertiaTensor(Matrix4x4 iitBody, Matrix4x4 xform, ref Matrix4x4 iitWorld)
        {
            var t1 = xform.M11 * iitBody.M11 +
                xform.M12 * iitBody.M21 +
                xform.M13 * iitBody.M31;
            var t2 = xform.M11 * iitBody.M12 +
                xform.M12 * iitBody.M22 +
                xform.M13 * iitBody.M32;
            var t3 = xform.M11 * iitBody.M13 +
                xform.M12 * iitBody.M23 +
                xform.M13 * iitBody.M33;
            var t4 = xform.M21 * iitBody.M11 +
                xform.M22 * iitBody.M21 +
                xform.M23 * iitBody.M31;
            var t5 = xform.M21 * iitBody.M12 +
                xform.M22 * iitBody.M22 +
                xform.M23 * iitBody.M32;
            var t6 = xform.M21 * iitBody.M13 +
                xform.M22 * iitBody.M23 +
                xform.M23 * iitBody.M33;
            var t7 = xform.M31 * iitBody.M11 +
                xform.M32 * iitBody.M21 +
                xform.M33 * iitBody.M31;
            var t8 = xform.M31 * iitBody.M12 +
                xform.M32 * iitBody.M22 +
                xform.M33 * iitBody.M32;
            var t9 = xform.M31 * iitBody.M13 +
                xform.M32 * iitBody.M23 +
                xform.M33 * iitBody.M33;

            iitWorld.M11 = t1 * xform.M11 + t2 * xform.M12 + t3 * xform.M13;
            iitWorld.M12 = t1 * xform.M21 + t2 * xform.M22 + t3 * xform.M23;
            iitWorld.M13 = t1 * xform.M31 + t2 * xform.M32 + t3 * xform.M33;
            iitWorld.M21 = t4 * xform.M11 + t5 * xform.M12 + t6 * xform.M13;
            iitWorld.M22 = t4 * xform.M21 + t5 * xform.M22 + t6 * xform.M23;
            iitWorld.M23 = t4 * xform.M31 + t5 * xform.M32 + t6 * xform.M33;
            iitWorld.M31 = t7 * xform.M11 + t8 * xform.M12 + t9 * xform.M13;
            iitWorld.M32 = t7 * xform.M21 + t8 * xform.M22 + t9 * xform.M23;
            iitWorld.M33 = t7 * xform.M31 + t8 * xform.M32 + t9 * xform.M33;
        }
    }
}
