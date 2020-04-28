using OpenH2.Core.Architecture;
using OpenH2.Foundation.Physics;
using OpenH2.Physics.Proxying;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace OpenH2.Engine.Components
{
    public class RigidBodyComponent : Component, IRigidBody
    {
        public IPhysicsProxy PhysicsImplementation { get; set; } = NullPhysicsProxy.Instance;
        public ICollider Collider { get; set; }
        public ITransform Transform { get; }

        public bool IsDynamic { get; private set; }
        private float mass;
        private float inverseMass;
        public float Mass { get => mass; private set { mass = value; inverseMass = 1 / value; } }
        public float InverseMass => inverseMass;
        public Vector3 CenterOfMass { get; private set; }
        public Matrix4x4 InertiaTensor { get; private set; }

        public Vector3 Velocity => this.PhysicsImplementation.GetVelocity();
        public Vector3 AngularVelocity => this.PhysicsImplementation.GetAngularVelocity();

        /// <summary>
        /// Creates a dynamic RigidBodyComponent
        /// </summary>
        public RigidBodyComponent(Entity parent, 
            TransformComponent xform, 
            Matrix4x4 inertiaTensor,
            float mass,
            Vector3 centerOfMassOffset = default)
            : base(parent)
        {
            this.Transform = xform;
            this.CenterOfMass = centerOfMassOffset;
            this.Mass = mass;

            // Set non-rotational components of matrix to standard values to ensure inversion succeeds
            // TODO: investigate if these components of the matrix have any meaningful data
            inertiaTensor.M14 = 0;
            inertiaTensor.M24 = 0;
            inertiaTensor.M34 = 0;
            inertiaTensor.M41 = 0;
            inertiaTensor.M42 = 0;
            inertiaTensor.M43 = 0;
            inertiaTensor.M44 = 1;

            this.InertiaTensor = inertiaTensor;
            this.IsDynamic = true;
        }

        /// <summary>
        /// Creates a static RigidBodyComponent
        /// </summary>
        public RigidBodyComponent(Entity parent, TransformComponent xform) : base(parent)
        {
            this.Transform = xform;
            this.CenterOfMass = Vector3.Zero;
            this.InertiaTensor = Matrix4x4.Identity;
            this.Mass = 0f;
            this.IsDynamic = false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UseTransform(Matrix4x4 xform)
        {
            this.Transform.UseTransformationMatrix(xform);
            this.PhysicsImplementation.UseTransformationMatrix(xform);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddForce(Vector3 force) => 
            this.PhysicsImplementation.AddForce(force);


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddForceAtPoint(Vector3 force, Vector3 point) => 
            this.PhysicsImplementation.AddForceAtPoint(force, point);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddForceAtLocalPoint(Vector3 force, Vector3 point) => 
            this.PhysicsImplementation.AddForceAtLocalPoint(force, point);
        
    }
}
