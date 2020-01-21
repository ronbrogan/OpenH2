using OpenH2.Foundation.Physics;
using System.Collections.Generic;
using System.Numerics;

namespace OpenH2.Physics.Colliders.Contacts
{
    public class ContactGenerator
    {
        public static readonly IList<Contact> Empty = new List<Contact>(0);

        public static IList<Contact> CollideBoxAndPlane(BoxCollider box, PlaneCollider plane)
        {
            var contacts = new List<Contact>(8);

            // Go through each combination of + and - for each half-size
            var mults = new float[,]{{1,1,1},{-1,1,1},{1,-1,1},{-1,-1,1},
                               {1,1,-1},{-1,1,-1},{1,-1,-1},{-1,-1,-1}};

            for (var i = 0; i < 8; i++)
            {
                // Calculate the position of each vertex
                var vertexPos = Vector3.Multiply(new Vector3(mults[i, 0], mults[i, 1], mults[i, 2]), box.HalfWidths);
                vertexPos = Vector3.Transform(vertexPos, box.Transform);

                float vertexDistance = Vector3.Dot(vertexPos, plane.Normal);

                // Check if vertex is "inside" plane's half space
                if (vertexDistance <= plane.Distance)
                {
                    // The contact point is halfway between the vertex and the
                    // plane - we multiply the direction by half the separation
                    // distance and add the vertex location.
                    Vector3 contactPoint = Vector3.Multiply(plane.Normal, (vertexDistance - plane.Distance));
                    contactPoint += vertexPos;

                    var contact = new Contact()
                    {
                        Point = contactPoint,
                        Normal = plane.Normal,
                        Penetration = plane.Distance - vertexDistance,
                        Friction = 1f,
                        Restitution = 0.1f
                    };

                    contacts.Add(contact);
                }
            }

            return contacts;
        }
    }
}
