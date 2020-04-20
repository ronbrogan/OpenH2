using System;
using System.Numerics;
using OpenH2.Foundation.Physics;

namespace OpenH2.Physics.Colliders.Tests
{
    public static class Gjk
    {

        const int GJK_MAX_NUM_ITERATIONS = 64;

        //Returns true if two colliders are intersecting. Has optional Minimum Translation Vector output param;
        //If supplied the EPA will be used to find the vector to separate coll1 from coll2
        public static bool Test(ISupportableCollider coll1, ISupportableCollider coll2, out Vector3 mtv)
        {
            Vector3 a, b, c, d = default; //Simplex: just a set of points (a is always most recently added)
            Vector3 search_dir = coll1.Position - coll2.Position; //initial search direction between colliders

            //Get initial point for simplex
            c = coll2.Support(search_dir, out _) - coll1.Support(-search_dir, out _);
            search_dir = -c; //search in direction of origin

            //Get second point for a line segment simplex
            b = coll2.Support(search_dir, out _) - coll1.Support(-search_dir, out _);

            if (Vector3.Dot(b, search_dir) < 0)
            {
                //we didn't reach the origin, won't enclose it
                mtv = default;
                return false;
            }

            search_dir = Vector3.Cross(Vector3.Cross(c - b, -b), c - b); //search perpendicular to line segment towards origin
            if (search_dir == new Vector3(0, 0, 0))
            { //origin is on this line segment
              //Apparently any normal search vector will do?
                search_dir = Vector3.Cross(c - b, new Vector3(1, 0, 0)); //normal with x-axis
                if (search_dir == new Vector3(0, 0, 0)) search_dir = Vector3.Cross(c - b, new Vector3(0, 0, -1)); //normal with z-axis
            }
            int simp_dim = 2; //simplex dimension

            for (int iterations = 0; iterations < GJK_MAX_NUM_ITERATIONS; iterations++)
            {
                a = coll2.Support(search_dir, out _) - coll1.Support(-search_dir, out _);
                if (Vector3.Dot(a, search_dir) < 0)
                {
                    //we didn't reach the origin, won't enclose it
                    mtv = default;
                    return false;
                }

                simp_dim++;
                if (simp_dim == 3)
                {
                    update_simplex3(ref a, ref b, ref c, ref d, ref simp_dim, ref search_dir);
                }
                else if (update_simplex4(ref a, ref b, ref c, ref d, ref simp_dim, ref search_dir))
                {
                    mtv = EPA(a, b, c, d, coll1, coll2);
                    return true;
                }
            }

            mtv = default;
            return false;
        }

        //Triangle case
        public static void update_simplex3(ref Vector3 a, ref Vector3 b, ref Vector3 c, ref Vector3 d, ref int simp_dim, ref Vector3 search_dir)
        {
            /* Required winding order:
            //  b
            //  | \
            //  |   \
            //  |    a
            //  |   /
            //  | /
            //  c
            */
            Vector3 n = Vector3.Cross(b - a, c - a); //triangle's normal
            Vector3 AO = -a; //direction to origin

            //Determine which feature is closest to origin, make that the new simplex

            simp_dim = 2;
            if (Vector3.Dot(Vector3.Cross(b - a, n), AO) > 0)
            { //Closest to edge AB
                c = a;
                //simp_dim = 2;
                search_dir = Vector3.Cross(Vector3.Cross(b - a, AO), b - a);
                return;
            }
            if (Vector3.Dot(Vector3.Cross(n, c - a), AO) > 0)
            { //Closest to edge AC
                b = a;
                //simp_dim = 2;
                search_dir = Vector3.Cross(Vector3.Cross(c - a, AO), c - a);
                return;
            }

            simp_dim = 3;
            if (Vector3.Dot(n, AO) > 0)
            { //Above triangle
                d = c;
                c = b;
                b = a;
                //simp_dim = 3;
                search_dir = n;
                return;
            }

            //else below triangle
            d = b;
            b = a;
            //simp_dim = 3;
            search_dir = -n;
            return;
        }

        //Tetrahedral case
        public static bool update_simplex4(ref Vector3 a, ref Vector3 b, ref Vector3 c, ref Vector3 d, ref int simp_dim, ref Vector3 search_dir)
        {
            // a is peak/tip of pyramid, BCD is the base (counterclockwise winding order)
            //We know a priori that origin is above BCD and below a

            //Get normals of three new faces
            Vector3 ABC = Vector3.Cross(b - a, c - a);
            Vector3 ACD = Vector3.Cross(c - a, d - a);
            Vector3 ADB = Vector3.Cross(d - a, b - a);

            Vector3 AO = -a; //dir to origin
            simp_dim = 3; //hoisting this just cause

            //Plane-test origin with 3 faces
            /*
            // Note: Kind of primitive approach used here; If origin is in front of a face, just use it as the new simplex.
            // We just go through the faces sequentially and exit at the first one which satisfies dot product. Not sure this 
            // is optimal or if edges should be considered as possible simplices? Thinking this through in my head I feel like 
            // this method is good enough. Makes no difference for AABBS, should test with more complex colliders.
            */
            if (Vector3.Dot(ABC, AO) > 0)
            { //In front of ABC
                d = c;
                c = b;
                b = a;
                search_dir = ABC;
                return false;
            }
            if (Vector3.Dot(ACD, AO) > 0)
            { //In front of ACD
                b = a;
                search_dir = ACD;
                return false;
            }
            if (Vector3.Dot(ADB, AO) > 0)
            { //In front of ADB
                c = d;
                d = b;
                b = a;
                search_dir = ADB;
                return false;
            }

            //else inside tetrahedron; enclosed!
            return true;

            //Note: in the case where two of the faces have similar normals,
            //The origin could conceivably be closest to an edge on the tetrahedron
            //Right now I don't think it'll make a difference to limit our new simplices
            //to just one of the faces, maybe test it later.
        }

        //Expanding Polytope Algorithm
        //Find minimum translation vector to resolve collision
        private const float EPA_TOLERANCE = 0.0001f;
        private const int EPA_MAX_NUM_FACES = 64;
        private const int EPA_MAX_NUM_LOOSE_EDGES = 32;
        private const int EPA_MAX_NUM_ITERATIONS = 64;


        //Expanding Polytope Algorithm. Used to find the mtv of two intersecting 
        //colliders using the final simplex obtained with the GJK algorithm
        public static Vector3 EPA(Vector3 a, Vector3 b, Vector3 c, Vector3 d, ISupportableCollider coll1, ISupportableCollider coll2)
        {
            Vector3[,] faces = new Vector3[EPA_MAX_NUM_FACES, 4]; //Array of faces, each with 3 verts and a normal

            //Init with final simplex from GJK
            faces[0, 0] = a;
            faces[0, 1] = b;
            faces[0, 2] = c;
            faces[0, 3] = Vector3.Normalize(Vector3.Cross(b - a, c - a)); //ABC
            faces[1, 0] = a;
            faces[1, 1] = c;
            faces[1, 2] = d;
            faces[1, 3] = Vector3.Normalize(Vector3.Cross(c - a, d - a)); //ACD
            faces[2, 0] = a;
            faces[2, 1] = d;
            faces[2, 2] = b;
            faces[2, 3] = Vector3.Normalize(Vector3.Cross(d - a, b - a)); //ADB
            faces[3, 0] = b;
            faces[3, 1] = d;
            faces[3, 2] = c;
            faces[3, 3] = Vector3.Normalize(Vector3.Cross(d - b, c - b)); //BDC

            int num_faces = 4;
            int closest_face = 0;

            for (int iterations = 0; iterations < EPA_MAX_NUM_ITERATIONS; iterations++)
            {
                //Find face that's closest to origin
                float min_dist = Vector3.Dot(faces[0, 0], faces[0, 3]);
                closest_face = 0;
                for (int i = 1; i < num_faces; i++)
                {
                    float dist = Vector3.Dot(faces[i, 0], faces[i, 3]);
                    if (dist < min_dist)
                    {
                        min_dist = dist;
                        closest_face = i;
                    }
                }

                //search normal to face that's closest to origin
                Vector3 search_dir = faces[closest_face, 3];
                Vector3 p = coll2.Support(search_dir, out _) - coll1.Support(-search_dir, out _);

                if (Vector3.Dot(p, search_dir) - min_dist < EPA_TOLERANCE)
                {
                    //Convergence (new point is not significantly further from origin)
                    return faces[closest_face, 3] * Vector3.Dot(p, search_dir); //dot vertex with normal to resolve collision along normal!
                }

                Vector3[,] loose_edges = new Vector3[EPA_MAX_NUM_LOOSE_EDGES, 2]; //keep track of edges we need to fix after removing faces
                int num_loose_edges = 0;

                //Find all triangles that are facing p
                for (int i = 0; i < num_faces; i++)
                {
                    if (Vector3.Dot(faces[i, 3], p - faces[i, 0]) > 0) //triangle i faces p, remove it
                    {
                        //Add removed triangle's edges to loose edge list.
                        //If it's already there, remove it (both triangles it belonged to are gone)
                        for (int j = 0; j < 3; j++) //Three edges per face
                        {
                            Vector3[] current_edge = new[] { faces[i, j], faces[i, (j + 1) % 3] };
                            bool found_edge = false;
                            for (int k = 0; k < num_loose_edges; k++) //Check if current edge is already in list
                            {
                                if (loose_edges[k, 1] == current_edge[0] && loose_edges[k, 0] == current_edge[1])
                                {
                                    //Edge is already in the list, remove it
                                    //THIS ASSUMES EDGE CAN ONLY BE SHARED BY 2 TRIANGLES (which should be true)
                                    //THIS ALSO ASSUMES SHARED EDGE WILL BE REVERSED IN THE TRIANGLES (which 
                                    //should be true provided every triangle is wound CCW)
                                    loose_edges[k, 0] = loose_edges[num_loose_edges - 1, 0]; //Overwrite current edge
                                    loose_edges[k, 1] = loose_edges[num_loose_edges - 1, 1]; //with last edge in list
                                    num_loose_edges--;
                                    found_edge = true;
                                    k = num_loose_edges; //exit loop because edge can only be shared once
                                }
                            }//endfor loose_edges

                            if (!found_edge)
                            { //add current edge to list
                              // assert(num_loose_edges<EPA_MAX_NUM_LOOSE_EDGES);
                                if (num_loose_edges >= EPA_MAX_NUM_LOOSE_EDGES) break;
                                loose_edges[num_loose_edges, 0] = current_edge[0];
                                loose_edges[num_loose_edges, 1] = current_edge[1];
                                num_loose_edges++;
                            }
                        }

                        //Remove triangle i from list
                        faces[i, 0] = faces[num_faces - 1, 0];
                        faces[i, 1] = faces[num_faces - 1, 1];
                        faces[i, 2] = faces[num_faces - 1, 2];
                        faces[i, 3] = faces[num_faces - 1, 3];
                        num_faces--;
                        i--;
                    }//endif p can see triangle i
                }//endfor num_faces

                //Reconstruct polytope with p added
                for (int i = 0; i < num_loose_edges; i++)
                {
                    // assert(num_faces<EPA_MAX_NUM_FACES);
                    if (num_faces >= EPA_MAX_NUM_FACES) break;
                    faces[num_faces, 0] = loose_edges[i, 0];
                    faces[num_faces, 1] = loose_edges[i, 1];
                    faces[num_faces, 2] = p;
                    faces[num_faces, 3] = Vector3.Normalize(Vector3.Cross(loose_edges[i, 0] - loose_edges[i, 1], loose_edges[i, 0] - p));

                    //Check for wrong normal to maintain CCW winding
                    float bias = 0.000001f; //in case dot result is only slightly < 0 (because origin is on face)
                    if (Vector3.Dot(faces[num_faces, 0], faces[num_faces, 3]) + bias < 0)
                    {
                        Vector3 temp = faces[num_faces, 0];
                        faces[num_faces, 0] = faces[num_faces, 1];
                        faces[num_faces, 1] = temp;
                        faces[num_faces, 3] = -faces[num_faces, 3];
                    }
                    num_faces++;
                }
            }

            //End for iterations
            Console.WriteLine("EPA did not converge");
            //Return most recent closest point
            return faces[closest_face, 3] * Vector3.Dot(faces[closest_face, 0], faces[closest_face, 3]);
        }
    }
}
