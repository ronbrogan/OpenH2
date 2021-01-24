using OpenH2.Foundation.Extensions;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace OpenH2.Foundation._3D
{
    public class QuickHull
    {
        private class Vertex
        {
            public Vector3 pnt { get; set; }
            public int index { get; set; }

            public Face face { get; set; }

            public Vertex next { get; set; }
            public Vertex prev { get; set; }
        }

        private class Face
        {
            public HalfEdge he0;

            private Vector3 normal;
            public double area;
            private Vector3 centroid;
            double planeOffset;
            int index;
            int numVerts;

            public Face next;

            public const int VISIBLE = 1;
            public const int NON_CONVEX = 2;
            public const int DELETED = 3;

            public int mark = VISIBLE;

            public Vertex outside;

            public void computeCentroid(ref Vector3 centroid)
            {
                centroid = Vector3.Zero;
                HalfEdge he = he0;
                do
                {
                    centroid += he.head().pnt;
                    he = he.next;
                }
                while (he != he0);
                centroid = Vector3.Multiply(centroid, 1f / numVerts);
            }

            public void computeNormal(ref Vector3 normal, double minArea)
            {
                computeNormal(ref normal);

                if (area < minArea)
                {
                    // make the normal more robust by removing
                    // components parallel to the longest edge

                    HalfEdge hedgeMax = null;
                    double lenSqrMax = 0;
                    HalfEdge hedge = he0;
                    do
                    {
                        double lenSqr = hedge.lengthSquared();
                        if (lenSqr > lenSqrMax)
                        {
                            hedgeMax = hedge;
                            lenSqrMax = lenSqr;
                        }
                        hedge = hedge.next;
                    }
                    while (hedge != he0);

                    Vector3 p2 = hedgeMax.head().pnt;
                    Vector3 p1 = hedgeMax.tail().pnt;
                    double lenMax = Math.Sqrt(lenSqrMax);
                    float ux = (float)((p2.X - p1.X) / lenMax);
                    float uy = (float)((p2.Y - p1.Y) / lenMax);
                    float uz = (float)((p2.Z - p1.Z) / lenMax);
                    float dot = (float)(normal.X * ux + normal.Y * uy + normal.Z * uz);
                    normal.X -= dot * ux;
                    normal.Y -= dot * uy;
                    normal.Z -= dot * uz;

                    normal = Vector3.Normalize(normal);
                }
            }

            public void computeNormal(ref Vector3 normal)
            {
                HalfEdge he1 = he0.next;
                HalfEdge he2 = he1.next;

                Vector3 p0 = he0.head().pnt;
                Vector3 p2 = he1.head().pnt;

                double d2x = p2.X - p0.X;
                double d2y = p2.Y - p0.Y;
                double d2z = p2.Z - p0.Z;

                normal = Vector3.Zero;

                numVerts = 2;

                while (he2 != he0)
                {
                    double d1x = d2x;
                    double d1y = d2y;
                    double d1z = d2z;

                    p2 = he2.head().pnt;
                    d2x = p2.X - p0.X;
                    d2y = p2.Y - p0.Y;
                    d2z = p2.Z - p0.Z;

                    normal.X += (float)(d1y * d2z - d1z * d2y);
                    normal.Y += (float)(d1z * d2x - d1x * d2z);
                    normal.Z += (float)(d1x * d2y - d1y * d2x);

                    he1 = he2;
                    he2 = he2.next;
                    numVerts++;
                }
                area = Vector3.Distance(normal, Vector3.Zero);
                normal = Vector3.Multiply(normal, (float)(1 / area));
            }

            private void computeNormalAndCentroid()
            {
                computeNormal(ref normal);
                computeCentroid(ref centroid);
                planeOffset = Vector3.Dot(normal, centroid);
                int numv = 0;
                HalfEdge he = he0;
                do
                {
                    numv++;
                    he = he.next;
                }
                while (he != he0);
                if (numv != numVerts)
                {
                    throw new Exception("face " + getVertexString() + " numVerts=" + numVerts + " should be " + numv);
                }
            }

            private void computeNormalAndCentroid(double minArea)
            {
                computeNormal(ref normal, minArea);
                computeCentroid(ref centroid);
                planeOffset = Vector3.Dot(normal, centroid);
            }

            public static Face createTriangle(Vertex v0, Vertex v1, Vertex v2)
            {
                return createTriangle(v0, v1, v2, 0);
            }

            /**
             * Constructs a triangule Face from vertices v0, v1, and v2.
             *
             * @param v0 first vertex
             * @param v1 second vertex
             * @param v2 third vertex
             */
            public static Face createTriangle(Vertex v0, Vertex v1, Vertex v2, double minArea)
            {
                Face face = new Face();
                HalfEdge he0 = new HalfEdge(v0, face);
                HalfEdge he1 = new HalfEdge(v1, face);
                HalfEdge he2 = new HalfEdge(v2, face);

                he0.prev = he2;
                he0.next = he1;
                he1.prev = he0;
                he1.next = he2;
                he2.prev = he1;
                he2.next = he0;

                face.he0 = he0;

                // compute the normal and offset
                face.computeNormalAndCentroid(minArea);
                return face;
            }

            public static Face create(Vertex[] vtxArray, int[] indices)
            {
                Face face = new Face();
                HalfEdge hePrev = null;
                for (int i = 0; i < indices.Length; i++)
                {
                    HalfEdge he = new HalfEdge(vtxArray[indices[i]], face);
                    if (hePrev != null)
                    {
                        he.setPrev(hePrev);
                        hePrev.setNext(he);
                    }
                    else
                    {
                        face.he0 = he;
                    }
                    hePrev = he;
                }
                face.he0.setPrev(hePrev);
                hePrev.setNext(face.he0);

                // compute the normal and offset
                face.computeNormalAndCentroid();
                return face;
            }

            public Face()
            {
                mark = VISIBLE;
            }

            /**
             * Gets the i-th half-edge associated with the face.
             * 
             * @param i the half-edge index, in the range 0-2.
             * @return the half-edge
             */
            public HalfEdge getEdge(int i)
            {
                HalfEdge he = he0;
                while (i > 0)
                {
                    he = he.next;
                    i--;
                }
                while (i < 0)
                {
                    he = he.prev;
                    i++;
                }
                return he;
            }

            public HalfEdge getFirstEdge()
            {
                return he0;
            }

            /**
             * Finds the half-edge within this face which has
             * tail <code>vt</code> and head <code>vh</code>.
             *
             * @param vt tail point
             * @param vh head point
             * @return the half-edge, or null if none is found.
             */
            public HalfEdge findEdge(Vertex vt, Vertex vh)
            {
                HalfEdge he = he0;
                do
                {
                    if (he.head() == vh && he.tail() == vt)
                    {
                        return he;
                    }
                    he = he.next;
                }
                while (he != he0);
                return null;
            }

            /**
             * Computes the distance from a point p to the plane of
             * this face.
             *
             * @param p the point
             * @return distance from the point to the plane
             */
            public double distanceToPlane(Vector3 p)
            {
                return normal.X * p.X + normal.Y * p.Y + normal.Z * p.Z - planeOffset;
            }

            /**
             * Returns the normal of the plane associated with this face.
             *
             * @return the planar normal
             */
            public Vector3 getNormal()
            {
                return normal;
            }

            public Vector3 getCentroid()
            {
                return centroid;
            }

            public int numVertices()
            {
                return numVerts;
            }

            public String getVertexString()
            {
                String s = null;
                HalfEdge he = he0;
                do
                {
                    if (s == null)
                    {
                        s = "" + he.head().index;
                    }
                    else
                    {
                        s += " " + he.head().index;
                    }
                    he = he.next;
                }
                while (he != he0);
                return s;
            }

            public void getVertexIndices(int[] idxs)
            {
                HalfEdge he = he0;
                int i = 0;
                do
                {
                    idxs[i++] = he.head().index;
                    he = he.next;
                }
                while (he != he0);
            }

            private Face connectHalfEdges(HalfEdge hedgePrev, HalfEdge hedge)
            {
                Face discardedFace = null;

                if (hedgePrev.oppositeFace() == hedge.oppositeFace())
                { // then there is a redundant edge that we can get rid off

                    Face oppFace = hedge.oppositeFace();
                    HalfEdge hedgeOpp;

                    if (hedgePrev == he0)
                    {
                        he0 = hedge;
                    }
                    if (oppFace.numVertices() == 3)
                    { // then we can get rid of the opposite face altogether
                        hedgeOpp = hedge.getOpposite().prev.getOpposite();

                        oppFace.mark = DELETED;
                        discardedFace = oppFace;
                    }
                    else
                    {
                        hedgeOpp = hedge.getOpposite().next;

                        if (oppFace.he0 == hedgeOpp.prev)
                        {
                            oppFace.he0 = hedgeOpp;
                        }
                        hedgeOpp.prev = hedgeOpp.prev.prev;
                        hedgeOpp.prev.next = hedgeOpp;
                    }
                    hedge.prev = hedgePrev.prev;
                    hedge.prev.next = hedge;

                    hedge.opposite = hedgeOpp;
                    hedgeOpp.opposite = hedge;

                    // oppFace was modified, so need to recompute
                    oppFace.computeNormalAndCentroid();
                }
                else
                {
                    hedgePrev.next = hedge;
                    hedge.prev = hedgePrev;
                }
                return discardedFace;
            }

            public void checkConsistency()
            {
                // do a sanity check on the face
                HalfEdge hedge = he0;
                double maxd = 0;
                int numv = 0;

                if (numVerts < 3)
                {
                    throw new Exception("degenerate face: " + getVertexString());
                }
                do
                {
                    HalfEdge hedgeOpp = hedge.getOpposite();
                    if (hedgeOpp == null)
                    {
                        throw new Exception(
                       "face " + getVertexString() + ": " +
                       "unreflected half edge " + hedge.getVertexString());
                    }
                    else if (hedgeOpp.getOpposite() != hedge)
                    {
                        throw new Exception(
                       "face " + getVertexString() + ": " +
                       "opposite half edge " + hedgeOpp.getVertexString() +
                       " has opposite " +
                       hedgeOpp.getOpposite().getVertexString());
                    }
                    if (hedgeOpp.head() != hedge.tail() ||
                    hedge.head() != hedgeOpp.tail())
                    {
                        throw new Exception(
                       "face " + getVertexString() + ": " +
                       "half edge " + hedge.getVertexString() +
                       " reflected by " + hedgeOpp.getVertexString());
                    }
                    Face oppFace = hedgeOpp.face;
                    if (oppFace == null)
                    {
                        throw new Exception(
                       "face " + getVertexString() + ": " +
                       "no face on half edge " + hedgeOpp.getVertexString());
                    }
                    else if (oppFace.mark == DELETED)
                    {
                        throw new Exception(
                       "face " + getVertexString() + ": " +
                       "opposite face " + oppFace.getVertexString() +
                       " not on hull");
                    }
                    double d = Math.Abs(distanceToPlane(hedge.head().pnt));
                    if (d > maxd)
                    {
                        maxd = d;
                    }
                    numv++;
                    hedge = hedge.next;
                }
                while (hedge != he0);

                if (numv != numVerts)
                {
                    throw new Exception("face " + getVertexString() + " numVerts=" + numVerts + " should be " + numv);
                }

            }

            public int mergeAdjacentFace(HalfEdge hedgeAdj,
                              Face[] discarded)
            {
                Face oppFace = hedgeAdj.oppositeFace();
                int numDiscarded = 0;

                discarded[numDiscarded++] = oppFace;
                oppFace.mark = DELETED;

                HalfEdge hedgeOpp = hedgeAdj.getOpposite();

                HalfEdge hedgeAdjPrev = hedgeAdj.prev;
                HalfEdge hedgeAdjNext = hedgeAdj.next;
                HalfEdge hedgeOppPrev = hedgeOpp.prev;
                HalfEdge hedgeOppNext = hedgeOpp.next;

                while (hedgeAdjPrev.oppositeFace() == oppFace)
                {
                    hedgeAdjPrev = hedgeAdjPrev.prev;
                    hedgeOppNext = hedgeOppNext.next;
                }

                while (hedgeAdjNext.oppositeFace() == oppFace)
                {
                    hedgeOppPrev = hedgeOppPrev.prev;
                    hedgeAdjNext = hedgeAdjNext.next;
                }

                HalfEdge hedge;

                for (hedge = hedgeOppNext; hedge != hedgeOppPrev.next; hedge = hedge.next)
                {
                    hedge.face = this;
                }

                if (hedgeAdj == he0)
                {
                    he0 = hedgeAdjNext;
                }

                // handle the half edges at the head
                Face discardedFace;

                discardedFace = connectHalfEdges(hedgeOppPrev, hedgeAdjNext);
                if (discardedFace != null)
                {
                    discarded[numDiscarded++] = discardedFace;
                }

                // handle the half edges at the tail
                discardedFace = connectHalfEdges(hedgeAdjPrev, hedgeOppNext);
                if (discardedFace != null)
                {
                    discarded[numDiscarded++] = discardedFace;
                }

                computeNormalAndCentroid();
                checkConsistency();

                return numDiscarded;
            }

            private double areaSquared(HalfEdge hedge0, HalfEdge hedge1)
            {
                // return the squared area of the triangle defined
                // by the half edge hedge0 and the point at the
                // head of hedge1.

                Vector3 p0 = hedge0.tail().pnt;
                Vector3 p1 = hedge0.head().pnt;
                Vector3 p2 = hedge1.head().pnt;

                double dx1 = p1.X - p0.X;
                double dy1 = p1.Y - p0.Y;
                double dz1 = p1.Z - p0.Z;

                double dx2 = p2.X - p0.X;
                double dy2 = p2.Y - p0.Y;
                double dz2 = p2.Z - p0.Z;

                double x = dy1 * dz2 - dz1 * dy2;
                double y = dz1 * dx2 - dx1 * dz2;
                double z = dx1 * dy2 - dy1 * dx2;

                return x * x + y * y + z * z;
            }

            public void triangulate(FaceList newFaces, double minArea)
            {
                HalfEdge hedge;

                if (numVertices() < 4)
                {
                    return;
                }

                Vertex v0 = he0.head();
                Face prevFace = null;

                hedge = he0.next;
                HalfEdge oppPrev = hedge.opposite;
                Face face0 = null;

                for (hedge = hedge.next; hedge != he0.prev; hedge = hedge.next)
                {
                    Face face =
                   createTriangle(v0, hedge.prev.head(), hedge.head(), minArea);
                    face.he0.next.setOpposite(oppPrev);
                    face.he0.prev.setOpposite(hedge.opposite);
                    oppPrev = face.he0;
                    newFaces.add(face);
                    if (face0 == null)
                    {
                        face0 = face;
                    }
                }
                hedge = new HalfEdge(he0.prev.prev.head(), this);
                hedge.setOpposite(oppPrev);

                hedge.prev = he0;
                hedge.prev.next = hedge;

                hedge.next = he0.prev;
                hedge.next.prev = hedge;

                computeNormalAndCentroid(minArea);
                checkConsistency();

                for (Face face = face0; face != null; face = face.next)
                {
                    face.checkConsistency();
                }

            }
        }

        private class HalfEdge
        {
            /**
             * The vertex associated with the head of this half-edge.
             */
            public Vertex vertex;

            /**
             * Triangular face associated with this half-edge.
             */
            public Face face;

            /**
             * Next half-edge in the triangle.
             */
            public HalfEdge next;

            /**
             * Previous half-edge in the triangle.
             */
            public HalfEdge prev;

            /**
             * Half-edge associated with the opposite triangle
             * adjacent to this edge.
             */
            public HalfEdge opposite;

            /**
             * Constructs a HalfEdge with head vertex <code>v</code> and
             * left-hand triangular face <code>f</code>.
             *
             * @param v head vertex
             * @param f left-hand triangular face
             */
            public HalfEdge(Vertex v, Face f)
            {
                vertex = v;
                face = f;
            }

            public HalfEdge()
            {
            }

            /**
             * Sets the value of the next edge adjacent
             * (counter-clockwise) to this one within the triangle.
             *
             * @param edge next adjacent edge */
            public void setNext(HalfEdge edge)
            {
                next = edge;
            }

            /**
             * Gets the value of the next edge adjacent
             * (counter-clockwise) to this one within the triangle.
             *
             * @return next adjacent edge */
            public HalfEdge getNext()
            {
                return next;
            }

            /**
             * Sets the value of the previous edge adjacent (clockwise) to
             * this one within the triangle.
             *
             * @param edge previous adjacent edge */
            public void setPrev(HalfEdge edge)
            {
                prev = edge;
            }

            /**
             * Gets the value of the previous edge adjacent (clockwise) to
             * this one within the triangle.
             *
             * @return previous adjacent edge
             */
            public HalfEdge getPrev()
            {
                return prev;
            }

            /**
             * Returns the triangular face located to the left of this
             * half-edge.
             *
             * @return left-hand triangular face
             */
            public Face getFace()
            {
                return face;
            }

            /**
             * Returns the half-edge opposite to this half-edge.
             *
             * @return opposite half-edge
             */
            public HalfEdge getOpposite()
            {
                return opposite;
            }

            /**
             * Sets the half-edge opposite to this half-edge.
             *
             * @param edge opposite half-edge
             */
            public void setOpposite(HalfEdge edge)
            {
                opposite = edge;
                edge.opposite = this;
            }

            /**
             * Returns the head vertex associated with this half-edge.
             *
             * @return head vertex
             */
            public Vertex head()
            {
                return vertex;
            }

            /**
             * Returns the tail vertex associated with this half-edge.
             *
             * @return tail vertex
             */
            public Vertex tail()
            {
                return prev != null ? prev.vertex : null;
            }

            /**
             * Returns the opposite triangular face associated with this
             * half-edge.
             *
             * @return opposite triangular face
             */
            public Face oppositeFace()
            {
                return opposite != null ? opposite.face : null;
            }

            /**
             * Produces a string identifying this half-edge by the point
             * index values of its tail and head vertices.
             *
             * @return identifying string
             */
            public String getVertexString()
            {
                if (tail() != null)
                {
                    return "" +
                   tail().index + "-" +
                   head().index;
                }
                else
                {
                    return "?-" + head().index;
                }
            }

            /**
             * Returns the length of this half-edge.
             *
             * @return half-edge length
             */
            public double length()
            {
                if (tail() != null)
                {
                    return Vector3.Distance(head().pnt, tail().pnt);
                }
                else
                {
                    return -1;
                }
            }

            /**
             * Returns the length squared of this half-edge.
             *
             * @return half-edge length squared
             */
            public double lengthSquared()
            {
                if (tail() != null)
                {
                    return Vector3.DistanceSquared(head().pnt, tail().pnt);
                }
                else
                {
                    return -1;
                }
            }
        }

        private const double AUTOMATIC_TOLERANCE = -1;
        private const double PRECISION = 2.2204460492503131e-8;
        // estimated size of the point set
        private double charLength;

        private int numVertices;
        private int numPoints;

        private readonly Vertex[] maxVtxs = new Vertex[3];
        private readonly Vertex[] minVtxs = new Vertex[3];

        private readonly List<Face> faces = new List<Face>(16);
        private readonly List<HalfEdge> horizon = new List<HalfEdge>(16);

        private readonly FaceList newFaces = new FaceList();
        private readonly VertexList unclaimed = new VertexList();
        private readonly VertexList claimed = new VertexList();

        private Vertex[] pointBuffer = new Vertex[0];
        private int[] vertexPointIndices = new int[0];
        private Face[] discardedFaces = new Face[3];

        /// <summary>
        /// Specifies that (on output) vertex indices for a face should be
        /// listed in clockwise order.
        /// </summary>
        public const int CLOCKWISE = 0x1;

        /// <summary>
        /// Specifies that (on output) the vertex indices for a face should be
        /// numbered with respect to the original input points.
        /// </summary>
        public const int POINT_RELATIVE = 0x8;

        /// <summary>
        /// Sets an explicit distance tolerance for convexity tests.
        /// If {@link #AUTOMATIC_TOLERANCE AUTOMATIC_TOLERANCE}
        /// is specified (the default), then the tolerance will be computed
        /// automatically from the point data.
        /// </summary>
        public double ExplicitTolerance { get; private set; } = AUTOMATIC_TOLERANCE;

        /// <summary>
        /// Returns the distance tolerance that was used for the most recently
        /// computed hull. The distance tolerance is used to determine when
        /// faces are unambiguously convex with respect to each other, and when
        /// points are unambiguously above or below a face plane, in the
        /// presence of numerical imprecision. Normally,
        /// this tolerance is computed automatically for each set of input
        /// points, but it can be set explicitly by the application.
        /// </summary>
        public double Tolerance { get; private set; }

        /// <summary>
        /// Creates a convex hull object and initializes it to the convex hull
        /// of a set of points whose coordinates are given by an
        /// array of doubles.
        /// </summary>
        /// <param name="coords"></param>
        public QuickHull(double[] coords)
        {
            var numPoints = coords.Length / 3;

            if (numPoints < 4)
            {
                throw new ArgumentException("Less than four input points specified");
            }

            InitBuffers(numPoints);
            SetPoints(coords, numPoints);
            BuildHull();
        }

        /// <summary>
        /// Creates a convex hull object and initializes it to the convex hull
        /// of a set of points.
        /// </summary>
        /// <param name="points"></param>
        public QuickHull(Vector3[] points)
        {
            var numPoints = points.Length;

            if (numPoints < 4)
            {
                throw new ArgumentException("Less than four input points specified");
            }

            InitBuffers(numPoints);
            SetPoints(points, numPoints);
            BuildHull();
        }

        /// <summary>
        /// Triangulates any non-triangular hull faces. In some cases, due to
        /// precision issues, the resulting triangles may be very thin or small,
        /// and hence appear to be non-convex (this same limitation is present
        /// in <a href=http://www.qhull.org>qhull</a>).
        /// </summary>
        public void Triangulate()
        {
            double minArea = 1000 * charLength * PRECISION;
            newFaces.Clear();

            foreach (var face in faces)
            {
                if (face.mark == Face.VISIBLE)
                {
                    face.triangulate(newFaces, minArea);
                }
            }

            for (Face face = newFaces.first(); face != null; face = face.next)
            {
                faces.Add(face);
            }
        }

        /// <summary>
        /// Returns an array specifing the index of each hull vertex
        /// with respect to the original input points.
        /// </summary>
        /// <returns></returns>
        public int[] GetVertexPointIndices()
        {
            int[] indices = new int[numVertices];
            for (int i = 0; i < numVertices; i++)
            {
                indices[i] = vertexPointIndices[i];
            }
            return indices;
        }

        /// <summary>
        /// Returns the number of faces in this hull.
        /// </summary>
        /// <returns></returns>
        public int GetNumFaces()
        {
            return faces.Count;
        }

        /// <summary>
        /// Returns the faces associated with this hull.
        /// 
        /// <p>Each face is represented by an integer array which gives the
        /// indices of the vertices. These indices are numbered
        /// relative to the
        /// hull vertices, are zero-based,
        /// and are arranged counter-clockwise. More control
        /// over the index format can be obtained using
        /// {@link #getFaces(int) getFaces(indexFlags)}.
        /// </summary>
        /// <returns></returns>
        public int[][] GetFaces()
        {
            return GetFaces(0);
        }

        /// <summary>
        /// Returns the faces associated with this hull.
        /// 
        /// <p>Each face is represented by an integer array which gives the
        /// indices of the vertices. By default, these indices are numbered with
        /// respect to the hull vertices (as opposed to the input points), are
        /// zero-based, and are arranged counter-clockwise. However, this
        /// can be changed by setting {@link #POINT_RELATIVE
        /// POINT_RELATIVE}, {@link #INDEXED_FROM_ONE INDEXED_FROM_ONE}, or
        /// {@link #CLOCKWISE CLOCKWISE} in the indexFlags parameter.
        /// 
        /// </summary>
        /// <param name="indexFlags"></param>
        /// <returns></returns>
        public int[][] GetFaces(int indexFlags)
        {
            int[][] allFaces = new int[faces.Count][];
            int k = 0;
            foreach (var face in faces)
            {
                allFaces[k] = new int[face.numVertices()];
                GetFaceIndices(allFaces[k], face, indexFlags);
                k++;
            }
            return allFaces;
        }

        protected void InitBuffers(int nump)
        {
            Vertex[] newBuffer = new Vertex[nump];
            vertexPointIndices = new int[nump];
            
            for (int i = 0; i < nump; i++)
            {
                newBuffer[i] = new Vertex();
            }

            pointBuffer = newBuffer;
            
            faces.Clear();
            numPoints = nump;
        }

        protected void SetPoints(double[] coords, int nump)
        {
            for (int i = 0; i < nump; i++)
            {
                Vertex vtx = pointBuffer[i];
                vtx.pnt = new Vector3((float)coords[i * 3 + 0], (float)coords[i * 3 + 1], (float)coords[i * 3 + 2]);
                vtx.index = i;
            }
        }

        protected void SetPoints(Vector3[] pnts, int nump)
        {
            for (int i = 0; i < nump; i++)
            {
                Vertex vtx = pointBuffer[i];
                vtx.pnt = pnts[i];
                vtx.index = i;
            }
        }

        /// <summary>
        /// Creates the initial simplex from which the hull will be built.
        /// </summary>
        protected void CreateInitialSimplex()
        {
            double max = 0;
            int imax = 0;

            for (int i = 0; i < 3; i++)
            {
                double diff = maxVtxs[i].pnt.Get(i) - minVtxs[i].pnt.Get(i);
                if (diff > max)
                {
                    max = diff;
                    imax = i;
                }
            }

            if (max <= Tolerance)
            {
                throw new ArgumentException("Input points appear to be coincident");
            }

            var vtx = new Vertex[4];
            // set first two vertices to be those with the greatest
            // one dimensional separation

            vtx[0] = maxVtxs[imax];
            vtx[1] = minVtxs[imax];

            // set third vertex to be the vertex farthest from
            // the line between vtx0 and vtx1
            Vector3 u01 = new Vector3();
            Vector3 nrml = new Vector3();
            Vector3 xprod = new Vector3();
            double maxSqr = 0;
            u01 = Vector3.Normalize(vtx[1].pnt - vtx[0].pnt);

            for (int i = 0; i < numPoints; i++)
            {
                xprod = Vector3.Cross(u01, pointBuffer[i].pnt - vtx[0].pnt);
                double lenSqr = Vector3.DistanceSquared(xprod, Vector3.Zero);
                if (lenSqr > maxSqr &&
                    pointBuffer[i] != vtx[0] &&  // paranoid
                    pointBuffer[i] != vtx[1])
                {
                    maxSqr = lenSqr;
                    vtx[2] = pointBuffer[i];
                    nrml = xprod;
                }
            }
            if (Math.Sqrt(maxSqr) <= 100 * Tolerance)
            {
                throw new ArgumentException("Input points appear to be colinear");
            }
            nrml = Vector3.Normalize(nrml);

            // recompute nrml to make sure it is normal to u10 - otherwise could
            // be errors in case vtx[2] is close to u10
            nrml -= Vector3.Multiply(Vector3.Dot(nrml, u01), u01); // component of nrml along u01
            nrml = Vector3.Normalize(nrml);

            double maxDist = 0;
            double d0 = Vector3.Dot(vtx[2].pnt, nrml);
            for (int i = 0; i < numPoints; i++)
            {
                double dist = Math.Abs(Vector3.Dot(pointBuffer[i].pnt, nrml) - d0);
                if (dist > maxDist &&
                    pointBuffer[i] != vtx[0] &&  // paranoid
                    pointBuffer[i] != vtx[1] &&
                    pointBuffer[i] != vtx[2])
                {
                    maxDist = dist;
                    vtx[3] = pointBuffer[i];
                }
            }

            if (Math.Abs(maxDist) <= 100 * Tolerance)
            {
                throw new ArgumentException("Input points appear to be coplanar");
            }

            var tris = new Face[4];

            if (Vector3.Dot(vtx[3].pnt, nrml) - d0 < 0)
            {
                tris[0] = Face.createTriangle(vtx[0], vtx[1], vtx[2]);
                tris[1] = Face.createTriangle(vtx[3], vtx[1], vtx[0]);
                tris[2] = Face.createTriangle(vtx[3], vtx[2], vtx[1]);
                tris[3] = Face.createTriangle(vtx[3], vtx[0], vtx[2]);

                for (int i = 0; i < 3; i++)
                {
                    int k = (i + 1) % 3;
                    tris[i + 1].getEdge(1).setOpposite(tris[k + 1].getEdge(0));
                    tris[i + 1].getEdge(2).setOpposite(tris[0].getEdge(k));
                }
            }
            else
            {
                tris[0] = Face.createTriangle(vtx[0], vtx[2], vtx[1]);
                tris[1] = Face.createTriangle(vtx[3], vtx[0], vtx[1]);
                tris[2] = Face.createTriangle(vtx[3], vtx[1], vtx[2]);
                tris[3] = Face.createTriangle(vtx[3], vtx[2], vtx[0]);

                for (int i = 0; i < 3; i++)
                {
                    int k = (i + 1) % 3;
                    tris[i + 1].getEdge(0).setOpposite(tris[k + 1].getEdge(1));
                    tris[i + 1].getEdge(2).setOpposite(tris[0].getEdge((3 - i) % 3));
                }
            }

            faces.Add(tris[0]);
            faces.Add(tris[1]);
            faces.Add(tris[2]);
            faces.Add(tris[3]);

            for (int i = 0; i < numPoints; i++)
            {
                Vertex v = pointBuffer[i];

                if (v == vtx[0] || v == vtx[1] || v == vtx[2] || v == vtx[3])
                {
                    continue;
                }

                maxDist = Tolerance;
                Face maxFace = null;

                for (int k = 0; k < 4; k++)
                {
                    double dist = tris[k].distanceToPlane(v.pnt);
                    if (dist > maxDist)
                    {
                        maxFace = tris[k];
                        maxDist = dist;
                    }
                }

                if (maxFace != null)
                {
                    AddPointToFace(v, maxFace);
                }
            }
        }

        protected void ComputeMaxAndMin()
        {
            Vector3 max;
            Vector3 min;

            for (int i = 0; i < 3; i++)
            {
                maxVtxs[i] = minVtxs[i] = pointBuffer[0];
            }

            max = pointBuffer[0].pnt;
            min = pointBuffer[0].pnt;

            for (int i = 1; i < numPoints; i++)
            {
                Vector3 pnt = pointBuffer[i].pnt;
                if (pnt.X > max.X)
                {
                    max.X = pnt.X;
                    maxVtxs[0] = pointBuffer[i];
                }
                else if (pnt.X < min.X)
                {
                    min.X = pnt.X;
                    minVtxs[0] = pointBuffer[i];
                }

                if (pnt.Y > max.Y)
                {
                    max.Y = pnt.Y;
                    maxVtxs[1] = pointBuffer[i];
                }
                else if (pnt.Y < min.Y)
                {
                    min.Y = pnt.Y;
                    minVtxs[1] = pointBuffer[i];
                }

                if (pnt.Z > max.Z)
                {
                    max.Z = pnt.Z;
                    maxVtxs[2] = pointBuffer[i];
                }
                else if (pnt.Z < min.Z)
                {
                    min.Z = pnt.Z;
                    minVtxs[2] = pointBuffer[i];
                }
            }

            // this epsilon formula comes from QuickHull, and I'm
            // not about to quibble.
            charLength = Math.Max(max.X - min.X, max.Y - min.Y);
            charLength = Math.Max(max.Z - min.Z, charLength);
            if (ExplicitTolerance == AUTOMATIC_TOLERANCE)
            {
                Tolerance =
                   3 * PRECISION * (Math.Max(Math.Abs(max.X), Math.Abs(min.X)) +
                                  Math.Max(Math.Abs(max.Y), Math.Abs(min.Y)) +
                                  Math.Max(Math.Abs(max.Z), Math.Abs(min.Z)));
            }
            else
            {
                Tolerance = ExplicitTolerance;
            }
        }

        private void GetFaceIndices(int[] indices, Face face, int flags)
        {
            bool ccw = ((flags & CLOCKWISE) == 0);
            bool pointRelative = ((flags & POINT_RELATIVE) != 0);

            HalfEdge hedge = face.he0;
            int k = 0;
            do
            {
                int idx = hedge.head().index;
                if (pointRelative)
                {
                    idx = vertexPointIndices[idx];
                }
                indices[k++] = idx;
                hedge = (ccw ? hedge.next : hedge.prev);
            }
            while (hedge != face.he0);
        }

        private void ResolveUnclaimedPoints(FaceList newFaces)
        {
            Vertex vtxNext = unclaimed.first();
            for (Vertex vtx = vtxNext; vtx != null; vtx = vtxNext)
            {
                vtxNext = vtx.next;

                double maxDist = Tolerance;
                Face maxFace = null;
                for (Face newFace = newFaces.first(); newFace != null;
                     newFace = newFace.next)
                {
                    if (newFace.mark == Face.VISIBLE)
                    {
                        double dist = newFace.distanceToPlane(vtx.pnt);
                        if (dist > maxDist)
                        {
                            maxDist = dist;
                            maxFace = newFace;
                        }
                        if (maxDist > 1000 * Tolerance)
                        {
                            break;
                        }
                    }
                }
                if (maxFace != null)
                {
                    AddPointToFace(vtx, maxFace);
                }
            }
        }

        private void DeleteFacePoints(Face face, Face absorbingFace)
        {
            Vertex faceVtxs = RemoveAllPointsFromFace(face);
            if (faceVtxs != null)
            {
                if (absorbingFace == null)
                {
                    unclaimed.addAll(faceVtxs);
                }
                else
                {
                    Vertex vtxNext = faceVtxs;
                    for (Vertex vtx = vtxNext; vtx != null; vtx = vtxNext)
                    {
                        vtxNext = vtx.next;
                        double dist = absorbingFace.distanceToPlane(vtx.pnt);
                        if (dist > Tolerance)
                        {
                            AddPointToFace(vtx, absorbingFace);
                        }
                        else
                        {
                            unclaimed.add(vtx);
                        }
                    }
                }
            }
        }

        private void AddPointToFace(Vertex vtx, Face face)
        {
            vtx.face = face;

            if(face.outside == null)
            {
                claimed.add(vtx);
            }
            else
            {
                claimed.insertBefore(vtx, face.outside);
            }

            face.outside = vtx;
        }

        private void RemovePointFromFace(Vertex vtx, Face face)
        {
            if (vtx == face.outside)
            {
                if (vtx.next != null && vtx.next.face == face)
                {
                    face.outside = vtx.next;
                }
                else
                {
                    face.outside = null;
                }
            }
            claimed.delete(vtx);
        }

        private Vertex RemoveAllPointsFromFace(Face face)
        {
            if (face.outside != null)
            {
                Vertex end = face.outside;
                while (end.next != null && end.next.face == face)
                {
                    end = end.next;
                }

                claimed.delete(face.outside, end);

                end.next = null;
                return face.outside;
            }
            else
            {
                return null;
            }
        }

        private HalfEdge FindHalfEdge(Vertex tail, Vertex head)
        {
            // brute force ... OK, since setHull is not used much
            foreach (var face in faces)
            {
                HalfEdge he = face.findEdge(tail, head);
                if (he != null)
                {
                    return he;
                }
            }
            return null;
        }


        private const int NONCONVEX_WRT_LARGER_FACE = 1;
        private const int NONCONVEX = 2;

        private double OppFaceDistance(HalfEdge he)
        {
            return he.face.distanceToPlane(he.opposite.face.getCentroid());
        }

        private bool DoAdjacentMerge(Face face, int mergeType)
        {
            HalfEdge hedge = face.he0;

            bool convex = true;
            do
            {
                Face oppFace = hedge.oppositeFace();
                bool merge = false;
                var hedgeDistance = OppFaceDistance(hedge);
                var oppositeHedgeDistance = OppFaceDistance(hedge.opposite);

                if (mergeType == NONCONVEX)
                {
                    // then merge faces if they are definitively non-convex
                    if (hedgeDistance > -Tolerance ||
                        oppositeHedgeDistance > -Tolerance)
                    {
                        merge = true;
                    }
                }
                else // mergeType == NONCONVEX_WRT_LARGER_FACE
                { 
                    // merge faces if they are parallel or non-convex
                    // wrt to the larger face; otherwise, just mark
                    // the face non-convex for the second pass.
                    if (face.area > oppFace.area)
                    {
                        if (hedgeDistance > -Tolerance)
                        {
                            merge = true;
                        }
                        else if (oppositeHedgeDistance > -Tolerance)
                        {
                            convex = false;
                        }
                    }
                    else
                    {
                        if (oppositeHedgeDistance > -Tolerance)
                        {
                            merge = true;
                        }
                        else if (hedgeDistance > -Tolerance)
                        {
                            convex = false;
                        }
                    }
                }

                if (merge)
                {

                    int numd = face.mergeAdjacentFace(hedge, discardedFaces);
                    for (int i = 0; i < numd; i++)
                    {
                        DeleteFacePoints(discardedFaces[i], face);
                    }

                    return true;
                }
                hedge = hedge.next;
            }
            while (hedge != face.he0);

            if (!convex)
            {
                face.mark = Face.NON_CONVEX;
            }

            return false;
        }

        private void CalculateHorizon(Vector3 eyePnt, HalfEdge edge0, Face face)
        {
            DeleteFacePoints(face, null);
            face.mark = Face.DELETED;

            HalfEdge edge;
            if (edge0 == null)
            {
                edge0 = face.getEdge(0);
                edge = edge0;
            }
            else
            {
                edge = edge0.getNext();
            }

            do
            {
                Face oppFace = edge.oppositeFace();
                if (oppFace.mark == Face.VISIBLE)
                {
                    if (oppFace.distanceToPlane(eyePnt) > Tolerance)
                    {
                        CalculateHorizon(eyePnt, edge.getOpposite(), oppFace);
                    }
                    else
                    {
                        horizon.Add(edge);
                    }
                }
                edge = edge.getNext();
            }
            while (edge != edge0);
        }

        private HalfEdge AddAdjoiningFace(Vertex eyeVtx, HalfEdge he)
        {
            var face = Face.createTriangle(eyeVtx, he.tail(), he.head());
            face.getEdge(-1).setOpposite(he.getOpposite());

            faces.Add(face);
            return face.getEdge(0);
        }

        private void AddNewFaces(Vertex eyeVtx)
        {
            this.newFaces.Clear();

            HalfEdge hedgeSidePrev = null;
            HalfEdge hedgeSideBegin = null;

            foreach (var horizonHe in horizon)
            {
                HalfEdge hedgeSide = AddAdjoiningFace(eyeVtx, horizonHe);

                if (hedgeSidePrev != null)
                {
                    hedgeSide.next.setOpposite(hedgeSidePrev);
                }
                else
                {
                    hedgeSideBegin = hedgeSide;
                }

                this.newFaces.add(hedgeSide.getFace());
                hedgeSidePrev = hedgeSide;
            }
            hedgeSideBegin.next.setOpposite(hedgeSidePrev);
        }

        private Vertex NextPointToAdd()
        {
            if (claimed.isEmpty())
            {
                return null;
            }
            else
            {
                Face eyeFace = claimed.first().face;
                Vertex eyeVtx = null;
                double maxDist = 0;
                for (Vertex vtx = eyeFace.outside;
                     vtx != null && vtx.face == eyeFace;
                     vtx = vtx.next)
                {
                    double dist = eyeFace.distanceToPlane(vtx.pnt);
                    if (dist > maxDist)
                    {
                        maxDist = dist;
                        eyeVtx = vtx;
                    }
                }
                return eyeVtx;
            }
        }

        private void AddPointToHull(Vertex eyeVtx)
        {
            horizon.Clear();
            unclaimed.Clear();

            RemovePointFromFace(eyeVtx, eyeVtx.face);
            CalculateHorizon(eyeVtx.pnt, null, eyeVtx.face);
            AddNewFaces(eyeVtx);

            // first merge pass ... merge faces which are non-convex
            // as determined by the larger face

            for (Face face = newFaces.first(); face != null; face = face.next)
            {
                if (face.mark == Face.VISIBLE)
                {
                    while (DoAdjacentMerge(face, NONCONVEX_WRT_LARGER_FACE))
                    { 
                    }
                }
            }
            // second merge pass ... merge faces which are non-convex
            // wrt either face      
            for (Face face = newFaces.first(); face != null; face = face.next)
            {
                if (face.mark == Face.NON_CONVEX)
                {
                    face.mark = Face.VISIBLE;

                    while (DoAdjacentMerge(face, NONCONVEX))
                    {
                    }
                }
            }

            ResolveUnclaimedPoints(newFaces);
        }

        protected void BuildHull()
        {
            Vertex eyeVtx;

            ComputeMaxAndMin();
            CreateInitialSimplex();
            while ((eyeVtx = NextPointToAdd()) != null)
            {
                AddPointToHull(eyeVtx);
            }
            ReindexFacesAndVertices();
        }

        private void MarkFaceVertices(Face face, int mark)
        {
            HalfEdge he0 = face.getFirstEdge();
            HalfEdge he = he0;
            do
            {
                he.head().index = mark;
                he = he.next;
            }
            while (he != he0);
        }

        protected void ReindexFacesAndVertices()
        {
            for (int i = 0; i < numPoints; i++)
            {
                pointBuffer[i].index = -1;
            }

            // remove inactive faces and mark active vertices
            var facesToProcess = faces.Count;
            for (int i = 0; i < facesToProcess; i++)
            {
                Face face = faces[i];
                if (face.mark != Face.VISIBLE)
                {
                    faces.RemoveAt(i);
                    i--;
                    facesToProcess--;
                }
                else
                {
                    MarkFaceVertices(face, 0);
                }
            }

            // reindex vertices
            numVertices = 0;
            for (int i = 0; i < numPoints; i++)
            {
                Vertex vtx = pointBuffer[i];
                if (vtx.index == 0)
                {
                    vertexPointIndices[numVertices] = i;
                    vtx.index = numVertices++;
                }
            }
        }

        private bool CheckFaceConvexity(Face face, double tol, StringBuilder ps)
        {
            double dist;
            HalfEdge he = face.he0;
            do
            {
                face.checkConsistency();
                // make sure edge is convex
                dist = OppFaceDistance(he);
                if (dist > tol)
                {
                    if (ps != null)
                    {
                        ps.AppendLine("Edge " + he.getVertexString() +
                                    " non-convex by " + dist);
                    }
                    return false;
                }
                dist = OppFaceDistance(he.opposite);
                if (dist > tol)
                {
                    if (ps != null)
                    {
                        ps.AppendLine("Opposite edge " +
                                    he.opposite.getVertexString() +
                                    " non-convex by " + dist);
                    }
                    return false;
                }
                if (he.next.oppositeFace() == he.oppositeFace())
                {
                    if (ps != null)
                    {
                        ps.AppendLine("Redundant vertex " + he.head().index +
                                    " in face " + face.getVertexString());
                    }
                    return false;
                }
                he = he.next;
            }
            while (he != face.he0);
            return true;
        }

        protected bool CheckFaces(double tol, StringBuilder ps)
        {
            // check edge convexity
            bool convex = true;
            foreach (var face in faces)
            {
                if (face.mark == Face.VISIBLE)
                {
                    if (!CheckFaceConvexity(face, tol, ps))
                    {
                        convex = false;
                    }
                }
            }
            return convex;
        }

        private class VertexList
        {
            private Vertex head;
            private Vertex tail;

            /**
             * Clears this list.
             */
            public void Clear()
            {
                head = tail = null;
            }

            /**
             * Adds a vertex to the end of this list.
             */
            public void add(Vertex vtx)
            {
                if (head == null)
                {
                    head = vtx;
                }
                else
                {
                    tail.next = vtx;
                }
                vtx.prev = tail;
                vtx.next = null;
                tail = vtx;
            }

            /**
             * Adds a chain of vertices to the end of this list.
             */
            public void addAll(Vertex vtx)
            {
                if (head == null)
                {
                    head = vtx;
                }
                else
                {
                    tail.next = vtx;
                }
                vtx.prev = tail;
                while (vtx.next != null)
                {
                    vtx = vtx.next;
                }
                tail = vtx;
            }

            /**
             * Deletes a vertex from this list.
             */
            public void delete(Vertex vtx)
            {
                if (vtx.prev == null)
                {
                    head = vtx.next;
                }
                else
                {
                    vtx.prev.next = vtx.next;
                }
                if (vtx.next == null)
                {
                    tail = vtx.prev;
                }
                else
                {
                    vtx.next.prev = vtx.prev;
                }
            }

            /**
             * Deletes a chain of vertices from this list.
             */
            public void delete(Vertex vtx1, Vertex vtx2)
            {
                if (vtx1.prev == null)
                {
                    head = vtx2.next;
                }
                else
                {
                    vtx1.prev.next = vtx2.next;
                }
                if (vtx2.next == null)
                {
                    tail = vtx1.prev;
                }
                else
                {
                    vtx2.next.prev = vtx1.prev;
                }
            }

            /**
             * Inserts a vertex into this list before another
             * specificed vertex.
             */
            public void insertBefore(Vertex vtx, Vertex next)
            {
                vtx.prev = next.prev;
                if (next.prev == null)
                {
                    head = vtx;
                }
                else
                {
                    next.prev.next = vtx;
                }
                vtx.next = next;
                next.prev = vtx;
            }

            /**
             * Returns the first element in this list.
             */
            public Vertex first()
            {
                return head;
            }

            /**
             * Returns true if this list is empty.
             */
            public bool isEmpty()
            {
                return head == null;
            }
        }

        private class FaceList
        {
            private Face head;
            private Face tail;

            /**
             * Clears this list.
             */
            public void Clear()
            {
                head = tail = null;
            }

            /**
             * Adds a vertex to the end of this list.
             */
            public void add(Face vtx)
            {
                if (head == null)
                {
                    head = vtx;
                }
                else
                {
                    tail.next = vtx;
                }
                vtx.next = null;
                tail = vtx;
            }

            public Face first()
            {
                return head;
            }

            /**
             * Returns true if this list is empty.
             */
            public bool isEmpty()
            {
                return head == null;
            }
        }

    }
}
