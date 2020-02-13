using OpenH2.Foundation.Extensions;
using OpenH2.Foundation.Physics;
using System.Numerics;

namespace OpenH2.Physics.Colliders.Contacts
{
    // REF Millington
    public class ContactResolver
    {
        private const float epsilon = 0.001f;
        private int velocityIterations = 0;
        private int positionIterations = 0;

        public ContactResolver(int iterations)
        {
            velocityIterations = iterations;
            positionIterations = iterations;
        }

        public void ResolveContacts(Contact[] contacts, float duration)
        {
            // Make sure we have something to do.
            if (contacts.Length == 0) return;

            // Prepare the contacts for processing
            PrepareContacts(contacts, duration);

            // Resolve the interpenetration problems with the contacts.
            AdjustPositions(contacts, duration);

            // Resolve the velocity problems with the contacts.
            AdjustVelocities(contacts, duration);
        }

        public void PrepareContacts(Contact[] contacts, float duration)
        {
            // Generate contact velocity and axis information.
            foreach (Contact contact in contacts)
            {
                // Calculate the internal contact data (inertia, basis, etc).
                contact.CalculateInternals(duration);
            }
        }

        public void AdjustVelocities(Contact[] c, float duration)
        {
            Vector3[] velocityChange = new Vector3[2], rotationChange = new Vector3[2];
            Vector3 deltaVel;

            // iteratively handle impacts in order of severity.
            var velocityIterationsUsed = 0;
            while (velocityIterationsUsed < velocityIterations)
            {
                // Find contact with maximum magnitude of probable velocity change.
                float max = epsilon;
                var index = c.Length;
                for (var i = 0; i < c.Length; i++)
                {
                    if (c[i].DesiredDeltaVelocity > max)
                    {
                        max = c[i].DesiredDeltaVelocity;
                        index = i;
                    }
                }
                if (index == c.Length) break;

                // Match the awake state at the contact
                c[index].MatchAwakeState();

                // Do the resolution on the contact that came out top.
                c[index].ApplyVelocityChange(velocityChange, rotationChange);

                // With the change in velocity of the two bodies, the update of
                // contact velocities means that some of the relative closing
                // velocities need recomputing.
                for (uint i = 0; i < c.Length; i++)
                {
                    var contact = c[i];
                    var body = new IRigidBody[] { contact.A as IRigidBody, contact.B as IRigidBody };
                    // Check each body in the contact
                    for (uint b = 0; b < 2; b++)
                    {
                        if (body[b] == null)
                            continue;

                        // Check for a match with each body in the newly
                        // resolved contact
                        for (uint d = 0; d < 2; d++)
                        {
                            var otherBody = d == 0 ? c[index].A : c[index].B;
                            if (body[b] == otherBody)
                            {
                                deltaVel = velocityChange[d] + Vector3.Cross(rotationChange[d], c[i].RelativeContactPosition[b]);

                                // The sign of the change is negative if we're dealing
                                // with the second body in a contact.
                                var transposed = Matrix4x4.Transpose(contact.ContactToWorld);
                                contact.ContactVelocity += Vector3.Transform(deltaVel, transposed) * (b > 0 ? -1 : 1);
                                contact.CalculateDesiredDeltaVelocity(duration);
                            }
                        }
                    }
                }

                velocityIterationsUsed++;
            }
        }

        public void AdjustPositions(Contact[] c, float duration)
        {
            int i, index;
            Vector3[] linearChange = new Vector3[2], angularChange = new Vector3[2];
            float max;
            Vector3 deltaPosition;

            // iteratively resolve interpenetrations in order of severity.
            var positionIterationsUsed = 0;
            while (positionIterationsUsed < positionIterations)
            {
                // Find biggest penetration
                max = epsilon;
                index = c.Length;
                for (i = 0; i < c.Length; i++)
                {
                    if (c[i].Penetration > max)
                    {
                        max = c[i].Penetration;
                        index = i;
                    }
                }
                if (index == c.Length) break;

                // Match the awake state at the contact
                c[index].MatchAwakeState();

                // Resolve the penetration.
                c[index].ApplyPositionChange(
                    linearChange,
                    angularChange,
                    max);

                // Again this action may have changed the penetration of other
                // bodies, so we update contacts.
                for (i = 0; i < c.Length; i++)
                {
                    var contact = c[i];
                    var body = new IRigidBody[] { contact.A as IRigidBody, contact.B as IRigidBody };
                    // Check each body in the contact
                    for (uint b = 0; b < 2; b++)
                    {
                        if (body[b] == null)
                            continue;

                        // Check for a match with each body in the newly
                        // resolved contact
                        for (uint d = 0; d < 2; d++)
                        {
                            var otherBody = d == 0 ? c[index].A : c[index].B;
                            if (body[b] == otherBody)
                            {
                                deltaPosition = linearChange[d] + Vector3.Cross(angularChange[d], c[i].RelativeContactPosition[b]);

                                // The sign of the change is positive if we're
                                // dealing with the second body in a contact
                                // and negative otherwise (because we're
                                // subtracting the resolution)..
                                c[i].Penetration += deltaPosition.ScalarProduct(c[i].Normal) * (b > 0 ? 1 : -1);
                            }
                        }
                    }
                }

                positionIterationsUsed++;
            }
        }
    }
}
