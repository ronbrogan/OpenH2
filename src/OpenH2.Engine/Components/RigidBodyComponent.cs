using OpenH2.Core.Architecture;
using OpenH2.Foundation.Physics;
using System.Numerics;

namespace OpenH2.Engine.Components
{
    public class RigidBodyComponent : Component, IRigidBody
    {
        public ICollider Collider { get; set; }
        public ITransform Transform { get; }

        private float mass;
        private float inverseMass;
        public float Mass { get => mass; private set { mass = value; inverseMass = 1 / value; } }
        public float InverseMass => inverseMass;
        public Vector3 CenterOfMass { get; private set; }
        public Matrix4x4 InertiaTensor { get; private set; }

        public Vector3 Velocity { get; }
        public Vector3 AngularVelocity { get; }

        public RigidBodyComponent(Entity parent, 
            TransformComponent xform, 
            Matrix4x4 inertiaTensor,
            Vector3 centerOfMassOffset = default)
            : base(parent)
        {
            this.Transform = xform;
            this.CenterOfMass = centerOfMassOffset;
            this.Mass = 20f;

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
        }
    }
}
