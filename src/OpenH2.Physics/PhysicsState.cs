using System;
using System.Numerics;

namespace OpenH2.Physics
{
    // https://gafferongames.com/post/physics_in_3d/
    public class PhysicsState
    {
        // "Primary" attributes
        public Vector3 Position { get; set; }
        public Vector3 Momentum { get; set; }
        public Quaternion Orientation { get; set; }
        public Vector3 AngularMomentum { get; set; }

        // "Secondary" or "calculated" attributes
        public Vector3 Velocity { get; set; }
        public Quaternion Spin { get; set; }
        public Vector3 AngularVelocity { get; set; }

        // "Constant" attributes
        private float mass;
        private float inverseMass;
        public float Mass { get => mass; set { mass = value; inverseMass = 1 / value; } } 
        public float InverseMass => inverseMass;
        private float inertia;
        private float inverseInertia;
        public float Inertia { get => inertia; set { inertia = value; inverseInertia = 1 / value; } }
        public float InverseInertia => inverseInertia;

        public void Recalculate()
        {
            Velocity = Momentum * InverseMass;

            AngularVelocity = AngularMomentum * InverseInertia;

            Orientation = Quaternion.Normalize(Orientation);

            var q = new Quaternion(0,
                          AngularVelocity.X,
                          AngularVelocity.Y,
                          AngularVelocity.Z);

            // Order of multiplies?
            Spin = Quaternion.Multiply(Quaternion.Multiply(Orientation, q), 0.5f);
        }
    }
}
