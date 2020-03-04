using OpenH2.Foundation.Physics;
using OpenH2.Physics.Colliders.Extensions;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace OpenH2.Physics.Colliders.Contacts
{
    /// <summary>
    /// Contact generation methods, mostly ported from Millington 2010
    /// </summary>
    public static class ContactGenerators
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
                vertexPos += box.OriginOffset;
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

        // TODO: change this to use OriginOffset of the box to determine points/centers
        public static IList<Contact> BoxAndBox(BoxCollider one, BoxCollider two)
        {
            var contacts = new List<Contact>(1);

            // Find the vector between the two centers
            Vector3 toCenter = two.GetAxis(3) - one.GetAxis(3);

            // We start assuming there is no contact
            float pen = float.MaxValue;
            int best = 0xffffff;

            // Now we check each axes, returning if it gives us
            // a separating axis, and keeping track of the axis with
            // the smallest penetration otherwise.
            if (!tryAxis(one, two, one.GetAxis(0), toCenter, 0, ref pen, ref best)) return contacts;
            if (!tryAxis(one, two, one.GetAxis(1), toCenter, 1, ref pen, ref best)) return contacts;
            if (!tryAxis(one, two, one.GetAxis(2), toCenter, 2, ref pen, ref best)) return contacts;

            if (!tryAxis(one, two, two.GetAxis(0), toCenter, 3, ref pen, ref best)) return contacts;
            if (!tryAxis(one, two, two.GetAxis(1), toCenter, 4, ref pen, ref best)) return contacts;
            if (!tryAxis(one, two, two.GetAxis(2), toCenter, 5, ref pen, ref best)) return contacts;

            // Store the best axis-major, in case we run into almost
            // parallel edge collisions later
            int bestSingleAxis = best;

            if (!tryAxis(one, two, Vector3.Cross(one.GetAxis(0), two.GetAxis(0)), toCenter, 6, ref pen, ref best)) return contacts;
            if (!tryAxis(one, two, Vector3.Cross(one.GetAxis(0), two.GetAxis(1)), toCenter, 7, ref pen, ref best)) return contacts;
            if (!tryAxis(one, two, Vector3.Cross(one.GetAxis(0), two.GetAxis(2)), toCenter, 8, ref pen, ref best)) return contacts;
            if (!tryAxis(one, two, Vector3.Cross(one.GetAxis(1), two.GetAxis(0)), toCenter, 9, ref pen, ref best)) return contacts;
            if (!tryAxis(one, two, Vector3.Cross(one.GetAxis(1), two.GetAxis(1)), toCenter, 10, ref pen, ref best)) return contacts;
            if (!tryAxis(one, two, Vector3.Cross(one.GetAxis(1), two.GetAxis(2)), toCenter, 11, ref pen, ref best)) return contacts;
            if (!tryAxis(one, two, Vector3.Cross(one.GetAxis(2), two.GetAxis(0)), toCenter, 12, ref pen, ref best)) return contacts;
            if (!tryAxis(one, two, Vector3.Cross(one.GetAxis(2), two.GetAxis(1)), toCenter, 13, ref pen, ref best)) return contacts;
            if (!tryAxis(one, two, Vector3.Cross(one.GetAxis(2), two.GetAxis(2)), toCenter, 14, ref pen, ref best)) return contacts;

            // Make sure we've got a result.
            if (best == 0xffffff) throw new System.Exception("No axis");

            // We now know there's a collision, and we know which
            // of the axes gave the smallest penetration. We now
            // can deal with it in different ways depending on
            // the case.
            if (best < 3)
            {
                // We've got a vertex of box two on a face of box one.
                contacts.Add(one.GetPointFaceBoxBoxContact(two, toCenter, best, pen));
            }
            else if (best < 6)
            {
                // We've got a vertex of box one on a face of box two.
                // We use the same algorithm as above, but swap around
                // one and two (and therefore also the vector between their
                // centers).
                contacts.Add(two.GetPointFaceBoxBoxContact(one, toCenter * -1.0f, best - 3, pen));
            }
            else
            {
                // We've got an edge-edge contact. Find out which axes
                best -= 6;
                int oneAxisIndex = best / 3;
                int twoAxisIndex = best % 3;
                Vector3 oneAxis = one.GetAxis(oneAxisIndex);
                Vector3 twoAxis = two.GetAxis(twoAxisIndex);
                Vector3 axis = Vector3.Cross(oneAxis, twoAxis);
                axis = Vector3.Normalize(axis);

                // The axis should point from box one to box two.
                if (Vector3.Dot(axis, toCenter) > 0)
                    axis = axis * -1.0f;

                // We have the axes, but not the edges: each axis has 4 edges parallel
                // to it, we need to find which of the 4 for each object. We do
                // that by finding the point in the center of the edge. We know
                // its component in the direction of the box's collision axis is zero
                // (its a mid-point) and we determine which of the extremes in each
                // of the other axes is closest.
                Vector3 ptOnOneEdge = one.HalfWidths;
                Vector3 ptOnTwoEdge = two.HalfWidths;
                for (var i = 0; i < 3; i++)
                {
                    if (i == oneAxisIndex)
                        ptOnOneEdge = ptOnOneEdge.Set(i, 0);
                    else if (Vector3.Dot(one.GetAxis(i), axis) > 0)
                        ptOnOneEdge = ptOnOneEdge.Set(i, -ptOnOneEdge.Get(i));

                    if (i == twoAxisIndex)
                        ptOnTwoEdge = ptOnTwoEdge.Set(i, 0);
                    else if (Vector3.Dot(two.GetAxis(i), axis) < 0)
                        ptOnTwoEdge = ptOnTwoEdge.Set(i, -ptOnTwoEdge.Get(i));
                }

                // Move them into world coordinates (they are already oriented
                // correctly, since they have been derived from the axes).
                ptOnOneEdge = Vector3.Transform(ptOnOneEdge, one.Transform);
                ptOnTwoEdge = Vector3.Transform(ptOnTwoEdge, two.Transform);

                // So we have a point and a direction for the colliding edges.
                // We need to find out point of closest approach of the two
                // line-segments.
                Vector3 vertex = NumericsExtensions.ContactPoint(
                    ptOnOneEdge, oneAxis, one.HalfWidths.Get(oneAxisIndex),
                    ptOnTwoEdge, twoAxis, two.HalfWidths.Get(twoAxisIndex),
                    bestSingleAxis > 2);

                // We can fill the contact.
                var contact = new Contact()
                {
                    Point = vertex,
                    Normal = axis,
                    Penetration = pen,
                    Friction = 0f,
                    Restitution = 0.1f
                };

                contacts.Add(contact);
            }

            return contacts;
        }

        private static bool tryAxis(
            BoxCollider one,
            BoxCollider two,
            Vector3 axis,
            Vector3 toCenter,
            int index,
            ref float smallestPenetration,
            ref int smallestCase
            )
        {
            // Make sure we have a normalized axis, and don't check almost parallel axes
            if (axis.LengthSquared() < 0.0001f) return true;

            axis = Vector3.Normalize(axis);

            float penetration = PenetrationOnAxis(one, two, axis, toCenter);

            if (penetration < 0) return false;
            if (penetration < smallestPenetration)
            {
                smallestPenetration = penetration;
                smallestCase = index;
            }
            return true;
        }

        public static float PenetrationOnAxis(BoxCollider one, BoxCollider two, Vector3 axis, Vector3 toCenter)
        {
            // Project the half-size of one onto axis
            float oneProject = one.TransformToAxis(axis);
            float twoProject = two.TransformToAxis(axis);

            // Project this onto the axis
            float distance = Math.Abs(Vector3.Dot(toCenter, axis));

            // Return the overlap (i.e. positive indicates
            // overlap, negative indicates separation).
            return oneProject + twoProject - distance;
        }

        // This method is called when we know that a vertex from
        // box two is in contact with box one.
        private static Contact GetPointFaceBoxBoxContact(this BoxCollider one, BoxCollider two, Vector3 toCenter, int best, float pen)
        {
            // We know which axis the collision is on (i.e. best),
            // but we need to work out which of the two faces on
            // this axis.
            Vector3 normal = one.GetAxis(best);
            if (Vector3.Dot(one.GetAxis(best), toCenter) > 0)
            {
                normal *= -1.0f;
            }

            // Work out which vertex of box two we're colliding with.
            // Using toCenter doesn't work!
            Vector3 vertex = two.HalfWidths;
            if (Vector3.Dot(two.GetAxis(0), normal) < 0) vertex.X = -vertex.X;
            if (Vector3.Dot(two.GetAxis(1), normal) < 0) vertex.Y = -vertex.Y;
            if (Vector3.Dot(two.GetAxis(2), normal) < 0) vertex.Z = -vertex.Z;

            // Create the contact data
            return new Contact
            {
                Normal = normal,
                Point = Vector3.Transform(vertex, two.Transform),
                Penetration = pen,
                Friction = 0f,
                Restitution = 0.1f,
            };
        }
    }
}
