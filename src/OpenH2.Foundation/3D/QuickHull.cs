using OpenH2.Foundation.Extensions;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace OpenH2.Foundation._3D
{
    public class QuickHull : IDisposable
    {
        private class Vertex
        {
            public Vector3 Point { get; set; }
            public int Index { get; set; }
            public Face Face { get; set; }
            public Vertex NextVertex { get; set; }
            public Vertex PreviousVertex { get; set; }
        }

        private class HalfEdge
        {
            /// <summary>
            /// The vertex associated with the head of this half-edge.
            /// </summary>
            public Vertex HeadVertex;
            public Vertex TailVertex => PreviousEdge != null ? PreviousEdge.HeadVertex : null;

            /// <summary>
            /// Triangular face associated with this half-edge.
            /// </summary>
            public Face Face { get; set; }

            /// <summary>
            /// Returns the opposite triangular face associated with this half-edge.
            /// </summary>
            public Face OppositeFace => OppositeEdge != null ? OppositeEdge.Face : null;

            /// <summary>
            /// Next half-edge in the triangle.
            /// </summary>
            public HalfEdge NextEdge { get; set; }

            /// <summary>
            /// Previous half-edge in the triangle.
            /// </summary>
            public HalfEdge PreviousEdge { get; set; }

            /// <summary>
            /// Half-edge associated with the opposite triangle adjacent to this edge.
            /// </summary>
            public HalfEdge OppositeEdge { get; private set; }

            /// <summary>
            /// Constructs a HalfEdge with head vertex <code>v</code> and
            /// left-hand triangular face <code>f</code>.
            /// </summary>
            public HalfEdge(Vertex v, Face f)
            {
                HeadVertex = v;
                Face = f;
            }

            /// <summary>
            /// Sets the half-edge opposite to this half-edge.
            /// </summary>
            /// <param name="edge"></param>
            public void SetOpposite(HalfEdge edge)
            {
                OppositeEdge = edge;
                edge.OppositeEdge = this;
            }

            /**
             * Produces a string identifying this half-edge by the point
             * index values of its tail and head vertices.
             *
             * @return identifying string
             */
            public string getVertexString()
            {
                if (TailVertex != null)
                {
                    return "" +
                   TailVertex.Index + "-" +
                   HeadVertex.Index;
                }
                else
                {
                    return "?-" + HeadVertex.Index;
                }
            }

            /**
             * Returns the length squared of this half-edge.
             *
             * @return half-edge length squared
             */
            public double LengthSquared()
            {
                if (TailVertex != null)
                {
                    return Vector3.DistanceSquared(HeadVertex.Point, TailVertex.Point);
                }
                else
                {
                    return -1;
                }
            }
        }

        private class Face
        {
            public const int VISIBLE = 1;
            public const int NON_CONVEX = 2;
            public const int DELETED = 3;

            private double planeOffset;
            public Vector3 Normal { get; private set; }
            public int VertexCount { get; private set; }
            public Vector3 Centroid { get; private set; }
            public HalfEdge HalfEdge { get; private set; }
            public double Area { get; private set; }
            public Face Next { get; set; }
            public int Mark { get; set; } = VISIBLE;
            public Vertex Outside { get; set; }

            public Face()
            {
                Mark = VISIBLE;
            }

            public void ComputeCentroid()
            {
                Centroid = Vector3.Zero;
                HalfEdge he = HalfEdge;
                do
                {
                    Centroid += he.HeadVertex.Point;
                    he = he.NextEdge;
                }
                while (he != HalfEdge);
                Centroid = Vector3.Multiply(Centroid, 1f / VertexCount);
            }

            public void ComputeNormal(double minArea)
            {
                ComputeNormal();

                if (Area < minArea)
                {
                    // make the normal more robust by removing
                    // components parallel to the longest edge

                    HalfEdge hedgeMax = null;
                    double lenSqrMax = 0;
                    HalfEdge hedge = HalfEdge;
                    do
                    {
                        double lenSqr = hedge.LengthSquared();
                        if (lenSqr > lenSqrMax)
                        {
                            hedgeMax = hedge;
                            lenSqrMax = lenSqr;
                        }
                        hedge = hedge.NextEdge;
                    }
                    while (hedge != HalfEdge);

                    Vector3 p2 = hedgeMax.HeadVertex.Point;
                    Vector3 p1 = hedgeMax.TailVertex.Point;
                    double lenMax = Math.Sqrt(lenSqrMax);
                    float ux = (float)((p2.X - p1.X) / lenMax);
                    float uy = (float)((p2.Y - p1.Y) / lenMax);
                    float uz = (float)((p2.Z - p1.Z) / lenMax);
                    float dot = (float)(this.Normal.X * ux + this.Normal.Y * uy + this.Normal.Z * uz);

                    var n = new Vector3();
                    n.X -= dot * ux;
                    n.Y -= dot * uy;
                    n.Z -= dot * uz;

                    this.Normal = Vector3.Normalize(n);
                }
            }

            public void ComputeNormal()
            {
                HalfEdge he1 = HalfEdge.NextEdge;
                HalfEdge he2 = he1.NextEdge;

                Vector3 p0 = HalfEdge.HeadVertex.Point;
                Vector3 p2 = he1.HeadVertex.Point;

                double d2x = p2.X - p0.X;
                double d2y = p2.Y - p0.Y;
                double d2z = p2.Z - p0.Z;

                var n = new Vector3();

                VertexCount = 2;

                while (he2 != HalfEdge)
                {
                    double d1x = d2x;
                    double d1y = d2y;
                    double d1z = d2z;

                    p2 = he2.HeadVertex.Point;
                    d2x = p2.X - p0.X;
                    d2y = p2.Y - p0.Y;
                    d2z = p2.Z - p0.Z;

                    n.X += (float)(d1y * d2z - d1z * d2y);
                    n.Y += (float)(d1z * d2x - d1x * d2z);
                    n.Z += (float)(d1x * d2y - d1y * d2x);

                    he1 = he2;
                    he2 = he2.NextEdge;
                    VertexCount++;
                }

                this.Area = Vector3.Distance(n, Vector3.Zero);
                this.Normal = Vector3.Multiply(n, (float)(1 / this.Area));
            }

            private void ComputeNormalAndCentroid()
            {
                ComputeNormal();
                ComputeCentroid();
                planeOffset = Vector3.Dot(Normal, Centroid);
                int numv = 0;
                HalfEdge he = HalfEdge;
                do
                {
                    numv++;
                    he = he.NextEdge;
                }
                while (he != HalfEdge);
                if (numv != VertexCount)
                {
                    throw new Exception("face " + GetVertexString() + " numVerts=" + VertexCount + " should be " + numv);
                }
            }

            private void ComputeNormalAndCentroid(double minArea)
            {
                ComputeNormal(minArea);
                ComputeCentroid();
                planeOffset = Vector3.Dot(Normal, Centroid);
            }

            public static Face CreateTriangle(Vertex v0, Vertex v1, Vertex v2)
            {
                return CreateTriangle(v0, v1, v2, 0);
            }

            public static Face CreateTriangle(Vertex v0, Vertex v1, Vertex v2, double minArea)
            {
                Face face = new Face();
                HalfEdge he0 = new HalfEdge(v0, face);
                HalfEdge he1 = new HalfEdge(v1, face);
                HalfEdge he2 = new HalfEdge(v2, face);

                he0.PreviousEdge = he2;
                he0.NextEdge = he1;
                he1.PreviousEdge = he0;
                he1.NextEdge = he2;
                he2.PreviousEdge = he1;
                he2.NextEdge = he0;

                face.HalfEdge = he0;

                // compute the normal and offset
                face.ComputeNormalAndCentroid(minArea);
                return face;
            }

            
            /**
             * Gets the i-th half-edge associated with the face.
             * 
             * @param i the half-edge index, in the range 0-2.
             * @return the half-edge
             */
            public HalfEdge GetEdge(int i)
            {
                HalfEdge he = HalfEdge;
                while (i > 0)
                {
                    he = he.NextEdge;
                    i--;
                }
                while (i < 0)
                {
                    he = he.PreviousEdge;
                    i++;
                }
                return he;
            }

            public HalfEdge GetFirstEdge()
            {
                return HalfEdge;
            }

            /**
             * Finds the half-edge within this face which has
             * tail <code>vt</code> and head <code>vh</code>.
             *
             * @param vt tail point
             * @param vh head point
             * @return the half-edge, or null if none is found.
             */
            public HalfEdge FindEdge(Vertex vt, Vertex vh)
            {
                HalfEdge he = HalfEdge;
                do
                {
                    if (he.HeadVertex == vh && he.TailVertex == vt)
                    {
                        return he;
                    }
                    he = he.NextEdge;
                }
                while (he != HalfEdge);
                return null;
            }

            /**
             * Computes the distance from a point p to the plane of
             * this face.
             *
             * @param p the point
             * @return distance from the point to the plane
             */
            public double DistanceToPlane(Vector3 p)
            {
                return Normal.X * p.X + Normal.Y * p.Y + Normal.Z * p.Z - planeOffset;
            }

            public string GetVertexString()
            {
                string s = null;
                HalfEdge he = HalfEdge;
                do
                {
                    if (s == null)
                    {
                        s = "" + he.HeadVertex.Index;
                    }
                    else
                    {
                        s += " " + he.HeadVertex.Index;
                    }
                    he = he.NextEdge;
                }
                while (he != HalfEdge);
                return s;
            }

            public void GetVertexIndices(int[] idxs)
            {
                HalfEdge he = HalfEdge;
                int i = 0;
                do
                {
                    idxs[i++] = he.HeadVertex.Index;
                    he = he.NextEdge;
                }
                while (he != HalfEdge);
            }

            private Face ConnectHalfEdges(HalfEdge hedgePrev, HalfEdge hedge)
            {
                Face discardedFace = null;

                if (hedgePrev.OppositeFace == hedge.OppositeFace)
                { // then there is a redundant edge that we can get rid off

                    Face oppFace = hedge.OppositeFace;
                    HalfEdge hedgeOpp;

                    if (hedgePrev == HalfEdge)
                    {
                        HalfEdge = hedge;
                    }
                    if (oppFace.VertexCount == 3)
                    { // then we can get rid of the opposite face altogether
                        hedgeOpp = hedge.OppositeEdge.PreviousEdge.OppositeEdge;

                        oppFace.Mark = DELETED;
                        discardedFace = oppFace;
                    }
                    else
                    {
                        hedgeOpp = hedge.OppositeEdge.NextEdge;

                        if (oppFace.HalfEdge == hedgeOpp.PreviousEdge)
                        {
                            oppFace.HalfEdge = hedgeOpp;
                        }
                        hedgeOpp.PreviousEdge = hedgeOpp.PreviousEdge.PreviousEdge;
                        hedgeOpp.PreviousEdge.NextEdge = hedgeOpp;
                    }
                    hedge.PreviousEdge = hedgePrev.PreviousEdge;
                    hedge.PreviousEdge.NextEdge = hedge;

                    hedge.SetOpposite(hedgeOpp);
                    hedgeOpp.SetOpposite(hedge);

                    // oppFace was modified, so need to recompute
                    oppFace.ComputeNormalAndCentroid();
                }
                else
                {
                    hedgePrev.NextEdge = hedge;
                    hedge.PreviousEdge = hedgePrev;
                }
                return discardedFace;
            }

            public void CheckConsistency()
            {
                // do a sanity check on the face
                HalfEdge hedge = HalfEdge;
                double maxd = 0;
                int numv = 0;

                if (VertexCount < 3)
                {
                    throw new Exception("degenerate face: " + GetVertexString());
                }
                do
                {
                    HalfEdge hedgeOpp = hedge.OppositeEdge;
                    if (hedgeOpp == null)
                    {
                        throw new Exception(
                       "face " + GetVertexString() + ": " +
                       "unreflected half edge " + hedge.getVertexString());
                    }
                    else if (hedgeOpp.OppositeEdge != hedge)
                    {
                        throw new Exception(
                       "face " + GetVertexString() + ": " +
                       "opposite half edge " + hedgeOpp.getVertexString() +
                       " has opposite " +
                       hedgeOpp.OppositeEdge.getVertexString());
                    }
                    if (hedgeOpp.HeadVertex != hedge.TailVertex ||
                    hedge.HeadVertex != hedgeOpp.TailVertex)
                    {
                        throw new Exception(
                       "face " + GetVertexString() + ": " +
                       "half edge " + hedge.getVertexString() +
                       " reflected by " + hedgeOpp.getVertexString());
                    }
                    Face oppFace = hedgeOpp.Face;
                    if (oppFace == null)
                    {
                        throw new Exception(
                       "face " + GetVertexString() + ": " +
                       "no face on half edge " + hedgeOpp.getVertexString());
                    }
                    else if (oppFace.Mark == DELETED)
                    {
                        throw new Exception(
                       "face " + GetVertexString() + ": " +
                       "opposite face " + oppFace.GetVertexString() +
                       " not on hull");
                    }
                    double d = Math.Abs(DistanceToPlane(hedge.HeadVertex.Point));
                    if (d > maxd)
                    {
                        maxd = d;
                    }
                    numv++;
                    hedge = hedge.NextEdge;
                }
                while (hedge != HalfEdge);

                if (numv != VertexCount)
                {
                    throw new Exception("face " + GetVertexString() + " numVerts=" + VertexCount + " should be " + numv);
                }

            }

            public int MergeAdjacentFace(HalfEdge hedgeAdj, Face[] discarded)
            {
                Face oppFace = hedgeAdj.OppositeFace;
                int numDiscarded = 0;

                discarded[numDiscarded++] = oppFace;
                oppFace.Mark = DELETED;

                HalfEdge hedgeOpp = hedgeAdj.OppositeEdge;

                HalfEdge hedgeAdjPrev = hedgeAdj.PreviousEdge;
                HalfEdge hedgeAdjNext = hedgeAdj.NextEdge;
                HalfEdge hedgeOppPrev = hedgeOpp.PreviousEdge;
                HalfEdge hedgeOppNext = hedgeOpp.NextEdge;

                while (hedgeAdjPrev.OppositeFace == oppFace)
                {
                    hedgeAdjPrev = hedgeAdjPrev.PreviousEdge;
                    hedgeOppNext = hedgeOppNext.NextEdge;
                }

                while (hedgeAdjNext.OppositeFace == oppFace)
                {
                    hedgeOppPrev = hedgeOppPrev.PreviousEdge;
                    hedgeAdjNext = hedgeAdjNext.NextEdge;
                }

                HalfEdge hedge;

                for (hedge = hedgeOppNext; hedge != hedgeOppPrev.NextEdge; hedge = hedge.NextEdge)
                {
                    hedge.Face = this;
                }

                if (hedgeAdj == HalfEdge)
                {
                    HalfEdge = hedgeAdjNext;
                }

                // handle the half edges at the head
                Face discardedFace;

                discardedFace = ConnectHalfEdges(hedgeOppPrev, hedgeAdjNext);
                if (discardedFace != null)
                {
                    discarded[numDiscarded++] = discardedFace;
                }

                // handle the half edges at the tail
                discardedFace = ConnectHalfEdges(hedgeAdjPrev, hedgeOppNext);
                if (discardedFace != null)
                {
                    discarded[numDiscarded++] = discardedFace;
                }

                ComputeNormalAndCentroid();
                CheckConsistency();

                return numDiscarded;
            }


            public void Triangulate(FaceList newFaces, double minArea)
            {
                HalfEdge hedge;

                if (VertexCount < 4)
                {
                    return;
                }

                Vertex v0 = HalfEdge.HeadVertex;
                Face prevFace = null;

                hedge = HalfEdge.NextEdge;
                HalfEdge oppPrev = hedge.OppositeEdge;
                Face face0 = null;

                for (hedge = hedge.NextEdge; hedge != HalfEdge.PreviousEdge; hedge = hedge.NextEdge)
                {
                    Face face =
                   CreateTriangle(v0, hedge.PreviousEdge.HeadVertex, hedge.HeadVertex, minArea);
                    face.HalfEdge.NextEdge.SetOpposite(oppPrev);
                    face.HalfEdge.PreviousEdge.SetOpposite(hedge.OppositeEdge);
                    oppPrev = face.HalfEdge;
                    newFaces.Add(face);
                    if (face0 == null)
                    {
                        face0 = face;
                    }
                }
                hedge = new HalfEdge(HalfEdge.PreviousEdge.PreviousEdge.HeadVertex, this);
                hedge.SetOpposite(oppPrev);

                hedge.PreviousEdge = HalfEdge;
                hedge.PreviousEdge.NextEdge = hedge;

                hedge.NextEdge = HalfEdge.PreviousEdge;
                hedge.NextEdge.PreviousEdge = hedge;

                ComputeNormalAndCentroid(minArea);
                CheckConsistency();

                for (Face face = face0; face != null; face = face.Next)
                {
                    face.CheckConsistency();
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

        private static ArrayPool<Vertex> pointPool = ArrayPool<Vertex>.Create();
        private static ArrayPool<int> indexPool = ArrayPool<int>.Create();

        private Vertex[] pointBuffer;
        private int[] vertexPointIndices;
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
                if (face.Mark == Face.VISIBLE)
                {
                    face.Triangulate(newFaces, minArea);
                }
            }

            for (Face face = newFaces.First(); face != null; face = face.Next)
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
                allFaces[k] = new int[face.VertexCount];
                GetFaceIndices(allFaces[k], face, indexFlags);
                k++;
            }
            return allFaces;
        }

        protected void InitBuffers(int nump)
        {
            Vertex[] newBuffer = pointPool.Rent(nump);
            vertexPointIndices = indexPool.Rent(nump);
            
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
                vtx.Point = new Vector3((float)coords[i * 3 + 0], (float)coords[i * 3 + 1], (float)coords[i * 3 + 2]);
                vtx.Index = i;
            }
        }

        protected void SetPoints(Vector3[] pnts, int nump)
        {
            for (int i = 0; i < nump; i++)
            {
                Vertex vtx = pointBuffer[i];
                vtx.Point = pnts[i];
                vtx.Index = i;
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
                double diff = maxVtxs[i].Point.Get(i) - minVtxs[i].Point.Get(i);
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
            u01 = Vector3.Normalize(vtx[1].Point - vtx[0].Point);

            for (int i = 0; i < numPoints; i++)
            {
                xprod = Vector3.Cross(u01, pointBuffer[i].Point - vtx[0].Point);
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
            double d0 = Vector3.Dot(vtx[2].Point, nrml);
            for (int i = 0; i < numPoints; i++)
            {
                double dist = Math.Abs(Vector3.Dot(pointBuffer[i].Point, nrml) - d0);
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

            if (Vector3.Dot(vtx[3].Point, nrml) - d0 < 0)
            {
                tris[0] = Face.CreateTriangle(vtx[0], vtx[1], vtx[2]);
                tris[1] = Face.CreateTriangle(vtx[3], vtx[1], vtx[0]);
                tris[2] = Face.CreateTriangle(vtx[3], vtx[2], vtx[1]);
                tris[3] = Face.CreateTriangle(vtx[3], vtx[0], vtx[2]);

                for (int i = 0; i < 3; i++)
                {
                    int k = (i + 1) % 3;
                    tris[i + 1].GetEdge(1).SetOpposite(tris[k + 1].GetEdge(0));
                    tris[i + 1].GetEdge(2).SetOpposite(tris[0].GetEdge(k));
                }
            }
            else
            {
                tris[0] = Face.CreateTriangle(vtx[0], vtx[2], vtx[1]);
                tris[1] = Face.CreateTriangle(vtx[3], vtx[0], vtx[1]);
                tris[2] = Face.CreateTriangle(vtx[3], vtx[1], vtx[2]);
                tris[3] = Face.CreateTriangle(vtx[3], vtx[2], vtx[0]);

                for (int i = 0; i < 3; i++)
                {
                    int k = (i + 1) % 3;
                    tris[i + 1].GetEdge(0).SetOpposite(tris[k + 1].GetEdge(1));
                    tris[i + 1].GetEdge(2).SetOpposite(tris[0].GetEdge((3 - i) % 3));
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
                    double dist = tris[k].DistanceToPlane(v.Point);
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

            max = pointBuffer[0].Point;
            min = pointBuffer[0].Point;

            for (int i = 1; i < numPoints; i++)
            {
                Vector3 pnt = pointBuffer[i].Point;
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

            HalfEdge hedge = face.HalfEdge;
            int k = 0;
            do
            {
                int idx = hedge.HeadVertex.Index;
                if (pointRelative)
                {
                    idx = vertexPointIndices[idx];
                }
                indices[k++] = idx;
                hedge = (ccw ? hedge.NextEdge : hedge.PreviousEdge);
            }
            while (hedge != face.HalfEdge);
        }

        private void ResolveUnclaimedPoints(FaceList newFaces)
        {
            Vertex vtxNext = unclaimed.First();
            for (Vertex vtx = vtxNext; vtx != null; vtx = vtxNext)
            {
                vtxNext = vtx.NextVertex;

                double maxDist = Tolerance;
                Face maxFace = null;
                for (Face newFace = newFaces.First(); newFace != null;
                     newFace = newFace.Next)
                {
                    if (newFace.Mark == Face.VISIBLE)
                    {
                        double dist = newFace.DistanceToPlane(vtx.Point);
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
                    unclaimed.AddAll(faceVtxs);
                }
                else
                {
                    Vertex vtxNext = faceVtxs;
                    for (Vertex vtx = vtxNext; vtx != null; vtx = vtxNext)
                    {
                        vtxNext = vtx.NextVertex;
                        double dist = absorbingFace.DistanceToPlane(vtx.Point);
                        if (dist > Tolerance)
                        {
                            AddPointToFace(vtx, absorbingFace);
                        }
                        else
                        {
                            unclaimed.Add(vtx);
                        }
                    }
                }
            }
        }

        private void AddPointToFace(Vertex vtx, Face face)
        {
            vtx.Face = face;

            if(face.Outside == null)
            {
                claimed.Add(vtx);
            }
            else
            {
                claimed.InsertBefore(vtx, face.Outside);
            }

            face.Outside = vtx;
        }

        private void RemovePointFromFace(Vertex vtx, Face face)
        {
            if (vtx == face.Outside)
            {
                if (vtx.NextVertex != null && vtx.NextVertex.Face == face)
                {
                    face.Outside = vtx.NextVertex;
                }
                else
                {
                    face.Outside = null;
                }
            }
            claimed.Delete(vtx);
        }

        private Vertex RemoveAllPointsFromFace(Face face)
        {
            if (face.Outside != null)
            {
                Vertex end = face.Outside;
                while (end.NextVertex != null && end.NextVertex.Face == face)
                {
                    end = end.NextVertex;
                }

                claimed.Delete(face.Outside, end);

                end.NextVertex = null;
                return face.Outside;
            }
            else
            {
                return null;
            }
        }

        private Vertex NextPointToAdd()
        {
            if (claimed.isEmpty())
            {
                return null;
            }
            else
            {
                Face eyeFace = claimed.First().Face;
                Vertex eyeVtx = null;
                double maxDist = 0;
                for (Vertex vtx = eyeFace.Outside;
                     vtx != null && vtx.Face == eyeFace;
                     vtx = vtx.NextVertex)
                {
                    double dist = eyeFace.DistanceToPlane(vtx.Point);
                    if (dist > maxDist)
                    {
                        maxDist = dist;
                        eyeVtx = vtx;
                    }
                }
                return eyeVtx;
            }
        }

        private HalfEdge FindHalfEdge(Vertex tail, Vertex head)
        {
            // brute force ... OK, since setHull is not used much
            foreach (var face in faces)
            {
                HalfEdge he = face.FindEdge(tail, head);
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
            return he.Face.DistanceToPlane(he.OppositeEdge.Face.Centroid);
        }

        private bool DoAdjacentMerge(Face face, int mergeType)
        {
            HalfEdge hedge = face.HalfEdge;

            bool convex = true;
            do
            {
                Face oppFace = hedge.OppositeFace;
                bool merge = false;
                var hedgeDistance = OppFaceDistance(hedge);
                var oppositeHedgeDistance = OppFaceDistance(hedge.OppositeEdge);

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
                    if (face.Area > oppFace.Area)
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

                    int numd = face.MergeAdjacentFace(hedge, discardedFaces);
                    for (int i = 0; i < numd; i++)
                    {
                        DeleteFacePoints(discardedFaces[i], face);
                    }

                    return true;
                }
                hedge = hedge.NextEdge;
            }
            while (hedge != face.HalfEdge);

            if (!convex)
            {
                face.Mark = Face.NON_CONVEX;
            }

            return false;
        }

        private void CalculateHorizon(Vector3 eyePnt, HalfEdge edge0, Face face)
        {
            DeleteFacePoints(face, null);
            face.Mark = Face.DELETED;

            HalfEdge edge;
            if (edge0 == null)
            {
                edge0 = face.GetEdge(0);
                edge = edge0;
            }
            else
            {
                edge = edge0.NextEdge;
            }

            do
            {
                Face oppFace = edge.OppositeFace;
                if (oppFace.Mark == Face.VISIBLE)
                {
                    if (oppFace.DistanceToPlane(eyePnt) > Tolerance)
                    {
                        CalculateHorizon(eyePnt, edge.OppositeEdge, oppFace);
                    }
                    else
                    {
                        horizon.Add(edge);
                    }
                }
                edge = edge.NextEdge;
            }
            while (edge != edge0);
        }

        private HalfEdge AddAdjoiningFace(Vertex eyeVtx, HalfEdge he)
        {
            var face = Face.CreateTriangle(eyeVtx, he.TailVertex, he.HeadVertex);
            face.GetEdge(-1).SetOpposite(he.OppositeEdge);

            faces.Add(face);
            return face.GetEdge(0);
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
                    hedgeSide.NextEdge.SetOpposite(hedgeSidePrev);
                }
                else
                {
                    hedgeSideBegin = hedgeSide;
                }

                this.newFaces.Add(hedgeSide.Face);
                hedgeSidePrev = hedgeSide;
            }
            hedgeSideBegin.NextEdge.SetOpposite(hedgeSidePrev);
        }

        private void AddPointToHull(Vertex eyeVtx)
        {
            horizon.Clear();
            unclaimed.Clear();

            RemovePointFromFace(eyeVtx, eyeVtx.Face);
            CalculateHorizon(eyeVtx.Point, null, eyeVtx.Face);
            AddNewFaces(eyeVtx);

            // first merge pass ... merge faces which are non-convex
            // as determined by the larger face

            for (Face face = newFaces.First(); face != null; face = face.Next)
            {
                if (face.Mark == Face.VISIBLE)
                {
                    while (DoAdjacentMerge(face, NONCONVEX_WRT_LARGER_FACE))
                    { 
                    }
                }
            }
            // second merge pass ... merge faces which are non-convex
            // wrt either face      
            for (Face face = newFaces.First(); face != null; face = face.Next)
            {
                if (face.Mark == Face.NON_CONVEX)
                {
                    face.Mark = Face.VISIBLE;

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
            HalfEdge he0 = face.GetFirstEdge();
            HalfEdge he = he0;
            do
            {
                he.HeadVertex.Index = mark;
                he = he.NextEdge;
            }
            while (he != he0);
        }

        protected void ReindexFacesAndVertices()
        {
            for (int i = 0; i < numPoints; i++)
            {
                pointBuffer[i].Index = -1;
            }

            // remove inactive faces and mark active vertices
            var facesToProcess = faces.Count;
            for (int i = 0; i < facesToProcess; i++)
            {
                Face face = faces[i];
                if (face.Mark != Face.VISIBLE)
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
                if (vtx.Index == 0)
                {
                    vertexPointIndices[numVertices] = i;
                    vtx.Index = numVertices++;
                }
            }
        }

        private bool CheckFaceConvexity(Face face, double tol, StringBuilder ps)
        {
            double dist;
            HalfEdge he = face.HalfEdge;
            do
            {
                face.CheckConsistency();
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
                dist = OppFaceDistance(he.OppositeEdge);
                if (dist > tol)
                {
                    if (ps != null)
                    {
                        ps.AppendLine("Opposite edge " +
                                    he.OppositeEdge.getVertexString() +
                                    " non-convex by " + dist);
                    }
                    return false;
                }
                if (he.NextEdge.OppositeFace == he.OppositeFace)
                {
                    if (ps != null)
                    {
                        ps.AppendLine("Redundant vertex " + he.HeadVertex.Index +
                                    " in face " + face.GetVertexString());
                    }
                    return false;
                }
                he = he.NextEdge;
            }
            while (he != face.HalfEdge);
            return true;
        }

        protected bool CheckFaces(double tol, StringBuilder ps)
        {
            // check edge convexity
            bool convex = true;
            foreach (var face in faces)
            {
                if (face.Mark == Face.VISIBLE)
                {
                    if (!CheckFaceConvexity(face, tol, ps))
                    {
                        convex = false;
                    }
                }
            }
            return convex;
        }

        private bool disposedValue;
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    pointPool?.Return(this.pointBuffer);
                    indexPool?.Return(this.vertexPointIndices);
                }

                this.pointBuffer = null;
                this.vertexPointIndices = null;
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
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

            /// <summary>
            /// Adds a vertex to the end of this list.
            /// </summary>
            public void Add(Vertex vtx)
            {
                if (head == null)
                {
                    head = vtx;
                }
                else
                {
                    tail.NextVertex = vtx;
                }
                vtx.PreviousVertex = tail;
                vtx.NextVertex = null;
                tail = vtx;
            }

            /// <summary>
            /// Adds a chain of vertices to the end of this list.
            /// </summary>
            public void AddAll(Vertex vtx)
            {
                if (head == null)
                {
                    head = vtx;
                }
                else
                {
                    tail.NextVertex = vtx;
                }
                vtx.PreviousVertex = tail;
                while (vtx.NextVertex != null)
                {
                    vtx = vtx.NextVertex;
                }
                tail = vtx;
            }

            /// <summary>
            /// Deletes a vertex from this list.
            /// </summary>
            public void Delete(Vertex vtx)
            {
                if (vtx.PreviousVertex == null)
                {
                    head = vtx.NextVertex;
                }
                else
                {
                    vtx.PreviousVertex.NextVertex = vtx.NextVertex;
                }
                if (vtx.NextVertex == null)
                {
                    tail = vtx.PreviousVertex;
                }
                else
                {
                    vtx.NextVertex.PreviousVertex = vtx.PreviousVertex;
                }
            }

            /// <summary>
            /// Deletes a chain of vertices from this list.
            /// </summary>
            public void Delete(Vertex from, Vertex to)
            {
                if (from.PreviousVertex == null)
                {
                    head = to.NextVertex;
                }
                else
                {
                    from.PreviousVertex.NextVertex = to.NextVertex;
                }
                if (to.NextVertex == null)
                {
                    tail = from.PreviousVertex;
                }
                else
                {
                    to.NextVertex.PreviousVertex = from.PreviousVertex;
                }
            }

            /// <summary>
            /// Inserts a vertex into this list before another specificed vertex.
            /// </summary>
            public void InsertBefore(Vertex vtx, Vertex next)
            {
                vtx.PreviousVertex = next.PreviousVertex;
                if (next.PreviousVertex == null)
                {
                    head = vtx;
                }
                else
                {
                    next.PreviousVertex.NextVertex = vtx;
                }
                vtx.NextVertex = next;
                next.PreviousVertex = vtx;
            }

            /// <summary>
            /// Returns the first element in this list.
            /// </summary>
            /// <returns></returns>
            public Vertex First()
            {
                return head;
            }

            /// <summary>
            /// Returns true if this list is empty.
            /// </summary>
            /// <returns></returns>
            public bool isEmpty()
            {
                return head == null;
            }
        }

        private class FaceList
        {
            private Face head;
            private Face tail;

            /// <summary>
            /// Clears this list.
            /// </summary>
            public void Clear()
            {
                head = tail = null;
            }

            /// <summary>
            /// Adds a vertex to the end of this list.
            /// </summary>
            /// <param name="vtx"></param>
            public void Add(Face vtx)
            {
                if (head == null)
                {
                    head = vtx;
                }
                else
                {
                    tail.Next = vtx;
                }
                vtx.Next = null;
                tail = vtx;
            }

            public Face First()
            {
                return head;
            }

            /// <summary>
            /// Returns true if this list is empty.
            /// </summary>
            public bool IsEmpty()
            {
                return head == null;
            }
        }
    }
}
