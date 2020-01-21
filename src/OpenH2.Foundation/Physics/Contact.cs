using OpenH2.Foundation.Extensions;
using System;
using System.Numerics;
using System.Threading;

namespace OpenH2.Foundation.Physics
{
    public class Contact
    {
        public IBody A { get; set; }
        public IBody B { get; set; }

        public Vector3 Point { get; set; }
        public Vector3 Normal { get; set; }
        public float Friction { get; set; }
        public float Penetration { get; set; }
        public float Restitution { get; set; }


        public Matrix4x4 ContactToWorld { get; set; }
        public Vector3 ContactVelocity { get; set; }
        public float DesiredDeltaVelocity { get; set; }
        public Vector3[] RelativeContactPosition { get; set; }

        public Contact()
        {
            ContactToWorld = Matrix4x4.Identity;
        }


        public void MatchAwakeState()
        {
            // Collisions with the world never cause a body to wake up.
            if (A == null) return;

            bool body0awake = A.IsAwake;
            bool body1awake = B.IsAwake;

            // Wake up only the sleeping one
            if (body0awake ^ body1awake)
            {
                if (body0awake)
                    B.IsAwake = true;
                else
                    A.IsAwake = true;
            }
        }

        /*
         * Swaps the bodies in the current contact, so body 0 is at body 1 and
         * vice versa. This also changes the direction of the contact normal,
         * but doesn't update any calculated internal data. If you are calling
         * this method manually, then call calculateInternals afterwards to
         * make sure the internal data is up to date.
         */
        public void SwapBodies()
        {
            Normal *= -1;

            var temp = A;
            A = B;
            B = temp;
        }

        /*
         * Constructs an arbitrary orthonormal basis for the contact.  This is
         * stored as a 3x3 matrix, where each vector is a column (in other
         * words the matrix transforms contact space into world space). The x
         * direction is generated from the contact normal, and the y and z
         * directionss are set so they are at right angles to it.
         */

        public void CalculateContactBasis()
        {
            Vector3[] contactTangent = new Vector3[2];

            // Check whether the Z-axis is nearer to the X or Y axis
            if (Math.Abs(Normal.X) > Math.Abs(Normal.Y))
            {
                // Scaling factor to ensure the results are normalised
                float s = 1.0f / (float)Math.Sqrt(Normal.Z * Normal.Z +
                    Normal.X * Normal.X);

                // The new X-axis is at right angles to the world Y-axis
                contactTangent[0].X = Normal.Z * s;
                contactTangent[0].Y = 0;
                contactTangent[0].Z = -Normal.X * s;

                // The new Y-axis is at right angles to the new X- and Z- axes
                contactTangent[1].X = Normal.Y * contactTangent[0].X;
                contactTangent[1].Y = Normal.Z * contactTangent[0].X -
                    Normal.X * contactTangent[0].Z;
                contactTangent[1].Z = -Normal.Y * contactTangent[0].X;
            }
            else
            {
                // Scaling factor to ensure the results are normalised
                float s = 1.0f / (float)Math.Sqrt(Normal.Z * Normal.Z +
                    Normal.Y * Normal.Y);

                // The new X-axis is at right angles to the world X-axis
                contactTangent[0].X = 0;
                contactTangent[0].Y = -Normal.Z * s;
                contactTangent[0].Z = Normal.Y * s;

                // The new Y-axis is at right angles to the new X- and Z- axes
                contactTangent[1].X = Normal.Y * contactTangent[0].Z -
                    Normal.Z * contactTangent[0].Y;
                contactTangent[1].Y = -Normal.X * contactTangent[0].Z;
                contactTangent[1].Z = Normal.X * contactTangent[0].Y;
            }

            // Make a matrix from the three vectors.
            ContactToWorld = ContactToWorld.SetComponents(
                Normal,
                contactTangent[0],
                contactTangent[1]);
        }

        public Vector3 CalculateLocalVelocity(uint bodyIndex, float duration)
        {
            var body = bodyIndex == 0 ? A : B;

            var thisBody = body as IRigidBody;

            if (thisBody == null)
            {
                return Vector3.Zero;
            }

            // Work out the velocity of the contact point.
            Vector3 velocity = Vector3.Cross(thisBody.AngularVelocity, this.RelativeContactPosition[bodyIndex]);
            velocity += thisBody.Velocity;

            // Turn the velocity into contact-coordinates.
            var transposed = Matrix4x4.Transpose(ContactToWorld);
            Vector3 ContactVelocity = Vector3.Transform(velocity, transposed);

            // Calculate the ammount of velocity that is due to forces without
            // reactions.
            Vector3 accVelocity = thisBody.PreviousAcceleration * duration;

            // Calculate the velocity in contact-coordinates.
            accVelocity = Vector3.Transform(accVelocity, transposed);

            // We ignore any component of acceleration in the contact normal
            // direction, we are only interested in planar acceleration
            accVelocity.X = 0;

            // Add the planar velocities - if there's enough Friction they will
            // be removed during velocity resolution
            ContactVelocity += accVelocity;

            // And return it
            return ContactVelocity;
        }


        public void CalculateDesiredDeltaVelocity(float duration)
        {
            float velocityLimit = (float)0.25f;

            // Calculate the acceleration induced velocity accumulated this frame
            float velocityFromAcc = 0;

            if (A.IsAwake && A is IRigidBody rigidA)
            {
                velocityFromAcc += Vector3.Dot((rigidA.PreviousAcceleration * duration), Normal);
            }

            if ((B?.IsAwake ?? false) && B is IRigidBody rigidB)
            {
                velocityFromAcc -= Vector3.Dot((rigidB.PreviousAcceleration * duration), Normal);
            }

            // If the velocity is very slow, limit the restitution
            float thisRestitution = Restitution;
            if (Math.Abs(ContactVelocity.X) < velocityLimit)
            {
                thisRestitution = (float)0.0f;
            }

            // Combine the bounce velocity with the removed
            // acceleration velocity.
            DesiredDeltaVelocity =
                -ContactVelocity.X
                - thisRestitution * (ContactVelocity.X - velocityFromAcc);
        }


        public void CalculateInternals(float duration)
        {
            this.RelativeContactPosition = new Vector3[2];

            // Check if the first object is NULL, and swap if it is.
            //if (!this.A) swapBodies();
            //assert(this.A);

            // Calculate an set of axis at the contact point.
            CalculateContactBasis();

            // Store the relative position of the contact relative to each body
            this.RelativeContactPosition[0] = this.Point - this.A.Transform.Position;
            if (this.B != null)
            {
                this.RelativeContactPosition[1] = this.Point - this.B.Transform.Position;
            }

            // Find the relative velocity of the bodies at the contact point.
            ContactVelocity = CalculateLocalVelocity(0, duration);
            if (this.B != null)
            {
                ContactVelocity -= CalculateLocalVelocity(1, duration);
            }

            // Calculate the desired change in velocity for resolution
            CalculateDesiredDeltaVelocity(duration);
        }

        public void ApplyVelocityChange(Vector3[] velocityChange, Vector3[] rotationChange)
        {
            var rigidA = this.A as IRigidBody;
            var rigidB = this.B as IRigidBody;

            // Get hold of the inverse mass and inverse inertia tensor, both in
            // world coordinates.
            var inverseInertiaTensor = new Matrix4x4[2];
            inverseInertiaTensor[0] = rigidA.InverseInertiaWorld;

            if (rigidB != null)
                inverseInertiaTensor[1] = rigidB.InverseInertiaWorld;

            // We will calculate the impulse for each contact axis
            Vector3 impulseContact;

            if (Friction == (float)0.0)
            {
                // Use the short format for Frictionless contacts
                impulseContact = CalculateFrictionlessImpulse(inverseInertiaTensor);
            }
            else
            {
                // Otherwise we may have impulses that aren't in the direction of the
                // contact, so we need the more complex version.
                impulseContact = CalculateFrictionImpulse(inverseInertiaTensor);
            }

            // Convert impulse to world coordinates
            Vector3 impulse = Vector3.Transform(impulseContact, ContactToWorld);

            // Split in the impulse into linear and rotational components
            Vector3 impulsiveTorque = Vector3.Cross(this.RelativeContactPosition[0], impulse);
            rotationChange[0] = Vector3.Transform(impulsiveTorque, inverseInertiaTensor[0]);


            //velocityChange[0].clear();
            velocityChange[0] = Vector3.Multiply(impulse, rigidA.InverseMass);

            // Apply the changes
            rigidA.AddVelocity(velocityChange[0]);
            rigidA.AddRotation(rotationChange[0]);

            if (rigidB != null)
            {
                // Work out body one's linear and angular changes
                Vector3 impulsiveTorqueB = Vector3.Cross(impulse, this.RelativeContactPosition[1]);
                rotationChange[1] = Vector3.Transform(impulsiveTorqueB, inverseInertiaTensor[1]);

                //velocityChange[1].clear();
                velocityChange[1] = Vector3.Multiply(impulse, -rigidB.InverseMass);

                // And apply them.
                rigidB.AddVelocity(velocityChange[1]);
                rigidB.AddRotation(rotationChange[1]);
            }
        }


        public Vector3 CalculateFrictionlessImpulse(Matrix4x4[] inverseInertiaTensor)
        {
            var rigidA = this.A as IRigidBody;
            var rigidB = this.B as IRigidBody;
            Vector3 impulseContact;

            // Build a vector that shows the change in velocity in
            // world space for a unit impulse in the direction of the contact
            // normal.
            Vector3 deltaVelWorld = Vector3.Cross(this.RelativeContactPosition[0], Normal);
            deltaVelWorld = Vector3.Transform(deltaVelWorld, inverseInertiaTensor[0]);
            deltaVelWorld = Vector3.Cross(deltaVelWorld, this.RelativeContactPosition[0]);

            // Work out the change in velocity in contact coordiantes.
            float deltaVelocity = Vector3.Dot(deltaVelWorld, Normal);

            // Add the linear component of velocity change
            deltaVelocity += rigidA.InverseMass;

            // Check if we need to the second body's data
            if (rigidB != null)
            {
                // Go through the same transformation sequence again
                Vector3 deltaVelWorldB = Vector3.Cross(this.RelativeContactPosition[1], Normal);
                deltaVelWorldB = Vector3.Transform(deltaVelWorldB, inverseInertiaTensor[1]);
                deltaVelWorldB = Vector3.Cross(deltaVelWorldB, this.RelativeContactPosition[1]);

                // Add the change in velocity due to rotation
                deltaVelocity += Vector3.Dot(deltaVelWorldB, Normal);

                // Add the change in velocity due to linear motion
                deltaVelocity += rigidB.InverseMass;
            }

            // Calculate the required size of the impulse
            impulseContact.X = DesiredDeltaVelocity / deltaVelocity;
            impulseContact.Y = 0;
            impulseContact.Z = 0;
            return impulseContact;
        }

        public Vector3 CalculateFrictionImpulse(Matrix4x4[] inverseInertiaTensor)
        {
            var rigidA = this.A as IRigidBody;
            var rigidB = this.B as IRigidBody;

            Vector3 impulseContact;
            float inverseMass = rigidA.InverseMass;

            // The equivalent of a cross product in matrices is multiplication
            // by a skew symmetric matrix - we build the matrix for converting
            // between linear and angular quantities.
            Matrix4x4 impulseToTorque = Matrix4x4.Identity;
            impulseToTorque = impulseToTorque.SetSkewSymmetric(this.RelativeContactPosition[0]);

            // Build the matrix to convert contact impulse to change in velocity
            // in world coordinates.
            Matrix4x4 deltaVelWorld = impulseToTorque;
            deltaVelWorld *= inverseInertiaTensor[0];
            deltaVelWorld *= impulseToTorque;
            deltaVelWorld *= -1;

            // Check if we need to add body two's data
            if (rigidB != null)
            {
                // Set the cross product matrix
                impulseToTorque = impulseToTorque.SetSkewSymmetric(this.RelativeContactPosition[1]);

                // Calculate the velocity change matrix
                Matrix4x4 deltaVelWorld2 = impulseToTorque;
                deltaVelWorld2 *= inverseInertiaTensor[1];
                deltaVelWorld2 *= impulseToTorque;
                deltaVelWorld2 *= -1;

                // Add to the total delta velocity.
                deltaVelWorld += deltaVelWorld2;

                // Add to the inverse mass
                inverseMass += rigidB.InverseMass;
            }

            // Do a change of basis to convert into contact coordinates.
            Matrix4x4 deltaVelocity = Matrix4x4.Transpose(ContactToWorld);
            deltaVelocity = Matrix4x4.Multiply(deltaVelocity, deltaVelWorld);
            deltaVelocity = Matrix4x4.Multiply(deltaVelocity, ContactToWorld);

            // Add in the linear velocity change
            deltaVelocity.M11 += inverseMass;
            deltaVelocity.M22 += inverseMass;
            deltaVelocity.M33 += inverseMass;

            deltaVelocity.M44 = 1f;

            // Invert to get the impulse needed per unit velocity
            Matrix4x4.Invert(deltaVelocity, out var impulseMatrix);

            // Find the target velocities to kill
            var velKill = new Vector3(DesiredDeltaVelocity, -ContactVelocity.Y, -ContactVelocity.Z);

            // Find the impulse to kill target velocities
            impulseContact = Vector3.Transform(velKill, impulseMatrix);

            // Check for exceeding Friction
            float planarImpulse = (float)Math.Sqrt(
                impulseContact.Y * impulseContact.Y +
                impulseContact.Z * impulseContact.Z);

            if (planarImpulse > impulseContact.X * Friction)
            {
                // We need to use dynamic Friction
                impulseContact.Y /= planarImpulse;
                impulseContact.Z /= planarImpulse;

                impulseContact.X = deltaVelocity.M11 +
                    deltaVelocity.M12 * Friction * impulseContact.Y +
                    deltaVelocity.M13 * Friction * impulseContact.Z;
                impulseContact.X = DesiredDeltaVelocity / impulseContact.X;
                impulseContact.Y *= Friction * impulseContact.X;
                impulseContact.Z *= Friction * impulseContact.X;
            }
            return impulseContact;
        }

        public void ApplyPositionChange(Vector3[] linearChange, Vector3[] angularChange, float penetration)
        {
            var angularLimit = 0.2f;
            var angularMove = new float[2];
            var linearMove = new float[2];

            var totalInertia = 0f;
            var linearInertia = new float[2];
            var angularInertia = new float[2];

            var body = new IRigidBody[] { this.A as IRigidBody, this.B as IRigidBody };

            // We need to work out the inertia of each object in the direction
            // of the contact normal, due to angular inertia only.
            for (uint i = 0; i < 2; i++) if (body[i] != null)
            {
                Matrix4x4 inverseInertiaTensor;
                inverseInertiaTensor = body[i].InverseInertiaWorld;

                // Use the same procedure as for calculating Frictionless
                // velocity change to work out the angular inertia.
                Vector3 angularInertiaWorld = Vector3.Cross(this.RelativeContactPosition[i], Normal);
                angularInertiaWorld = Vector3.Transform(angularInertiaWorld, inverseInertiaTensor);
                angularInertiaWorld = Vector3.Cross(angularInertiaWorld, this.RelativeContactPosition[i]);
                angularInertia[i] = Vector3.Dot(angularInertiaWorld, Normal);

                // The linear component is simply the inverse mass
                linearInertia[i] = body[i].InverseMass;

                // Keep track of the total inertia from all components
                totalInertia += linearInertia[i] + angularInertia[i];

                // We break the loop here so that the totalInertia value is
                // completely calculated (by both iterations) before
                // continuing.
            }

            // Loop through again calculating and applying the changes
            for (uint i = 0; i < 2; i++) if (body[i] != null)
            {
                // The linear and angular movements required are in proportion to
                // the two inverse inertias.
                float sign = (i == 0) ? 1 : -1;
                angularMove[i] =
                    sign * penetration * (angularInertia[i] / totalInertia);
                linearMove[i] =
                    sign * penetration * (linearInertia[i] / totalInertia);

                // To avoid angular projections that are too great (when mass is large
                // but inertia tensor is small) limit the angular move.
                Vector3 projection = this.RelativeContactPosition[i];
                projection += Vector3.Multiply(Normal, -this.RelativeContactPosition[i].ScalarProduct(Normal));

                // Use the small angle approximation for the sine of the angle (i.e.
                // the magnitude would be sine(angularLimit) * projection.magnitude
                // but we approximate sine(angularLimit) to angularLimit).
                float maxMagnitude = angularLimit * projection.Length();

                if (angularMove[i] < -maxMagnitude)
                {
                    float totalMove = angularMove[i] + linearMove[i];
                    angularMove[i] = -maxMagnitude;
                    linearMove[i] = totalMove - angularMove[i];
                }
                else if (angularMove[i] > maxMagnitude)
                {
                    float totalMove = angularMove[i] + linearMove[i];
                    angularMove[i] = maxMagnitude;
                    linearMove[i] = totalMove - angularMove[i];
                }

                // We have the linear amount of movement required by turning
                // the rigid body (in angularMove[i]). We now need to
                // calculate the desired rotation to achieve that.
                if (angularMove[i] == 0)
                {
                    // Easy case - no angular movement means no rotation.
                    angularChange[i] = Vector3.Zero;
                }
                else
                {
                    // Work out the direction we'd like to rotate in.
                    var targetAngularDirection = Vector3.Cross(this.RelativeContactPosition[i], Normal);

                    Matrix4x4 inverseInertiaTensor = body[i].InverseInertiaWorld;

                    // Work out the direction we'd need to rotate to achieve that
                    angularChange[i] = Vector3.Transform(targetAngularDirection, inverseInertiaTensor) 
                        * (angularMove[i] / angularInertia[i]);
                }

                // Velocity change is easier - it is just the linear movement
                // along the contact normal.
                linearChange[i] = Normal * linearMove[i];

                // Now we can start to apply the values we've calculated.
                // Apply the linear movement
                Vector3 pos = body[i].Transform.Position;
                pos += Vector3.Multiply(Normal, linearMove[i]);
                body[i].Transform.Position = pos;

                // And the change in orientation
                Quaternion q = body[i].Transform.Orientation;
                q = q.ApplyScaledVector(angularChange[i], 1.0f);
                body[i].Transform.Orientation = q;


                // We need to calculate the derived data for any body that is
                // asleep, so that the changes are reflected in the object's
                // data. Otherwise the resolution will not change the position
                // of the object, and the next collision detection round will
                // have the same penetration.
                if (!body[i].IsAwake) 
                    body[i].UpdateDerivedData();
            }
        }
    }
}
