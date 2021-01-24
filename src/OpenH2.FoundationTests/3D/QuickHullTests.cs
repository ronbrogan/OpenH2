using OpenH2.Foundation._3D;
using OpenH2.Foundation.Extensions;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Xunit;
using Xunit.Abstractions;

namespace OpenH2.Foundation.Tests._3D
{
    public class QuickHullTests
    {
        private const double DOUBLE_PREC = 2.2204460492503131e-16;

        static bool triangulate = false;
        static bool doTesting = true;
        static bool doTiming = false;

        static bool debugEnable = false;

        const int NO_DEGENERACY = 0;
        const int EDGE_DEGENERACY = 1;
        const int VERTEX_DEGENERACY = 2;
        private readonly ITestOutputHelper output;
        Random rand; // random number generator

        static bool testRotation = true;
        static int degeneracyTest = VERTEX_DEGENERACY;
        static double epsScale = 2.0;
        double[] coords = null;

        /**
         * Creates a testing object.
         */
        public QuickHullTests(ITestOutputHelper output)
        {
            rand = new Random(0x1234);
            this.output = output;
        }

        [Fact]
        public void Degenerates()
        {
            for (int dimen = 0; dimen < 3; dimen++)
            {
                for (int i = 0; i < 10; i++)
                {
                    coords = randomDegeneratePoints(10, dimen);
                    if (dimen == 0)
                    {
                        testException(coords, "Input points appear to be coincident");
                    }
                    else if (dimen == 1)
                    {
                        testException(coords, "Input points appear to be colinear");
                    }
                    else if (dimen == 2)
                    {
                        testException(coords, "Input points appear to be coplanar");
                    }
                }
            }
        }

        [Fact]
        public void CaseB()
        {
            coords = new double[]
                        {
                0.0 , 0.0 , 0.0,
                21.0, 0.0 , 0.0,
                0.0 , 21.0, 0.0,
                2.0 , 1.0 , 2.0,
                17.0, 2.0 , 3.0,
                1.0 , 19.0, 6.0,
                4.0 , 3.0 , 5.0,
                13.0, 4.0 , 5.0,
                3.0 , 15.0, 8.0,
                6.0 , 5.0 , 6.0,
                9.0 , 6.0 , 11.0,
                        };
            test(coords, null);
        }

        [Fact]
        public void CaseA()
        {
            coords = new double[]
                        {
                21, 0, 0,
                0, 21, 0,
                0, 0, 0,
                18, 2, 6,
                1, 18, 5,
                2, 1, 3,
                14, 3, 10,
                4, 14, 14,
                3, 4, 10,
                10, 6, 12,
                5, 10, 15,
                        };
            test(coords, null);
        }

        [Fact]
        public void PointGrid()
        {
            this.output.WriteLine("Testing 8 to 1000 randomly shuffled points on a grid ...");
            for (int n = 2; n <= 10; n++)
            {
                for (int i = 0; i < 10; i++)
                {
                    coords = randomGridPoints(n, 4.0);
                    test(coords, null);
                }
            }
        }

        [Fact]
        public void CubicPoints()
        {
            this.output.WriteLine("Testing 20 to 200 random points clipped to a cube ...");
            for (int n = 20; n < 200; n += 10)
            {
                for (int i = 0; i < 10; i++)
                {
                    coords = randomCubedPoints(n, 1.0, 0.5);
                    test(coords, null);
                }
            }
        }

        [Fact]
        public void SphericalPoints()
        {
            this.output.WriteLine("Testing 20 to 200 random points in a sphere ...");
            for (int n = 20; n < 200; n += 10)
            {
                for (int i = 0; i < 10; i++)
                {
                    coords = randomSphericalPoints(n, 1);
                    test(coords, null);
                }
            }
        }

        [Fact]
        public void RandomPoints()
        {
            this.output.WriteLine("Testing 20 to 200 random points ...");
            for (int n = 20; n < 200; n += 10)
            {
                for (int i = 0; i < 10; i++)
                {
                    coords = randomPoints(n, 1.0);
                    test(coords, null);
                }
            }
        }

        /**
         * Returns true if two face index sets are equal,
         * modulo a cyclical permuation.
         *
         * @param indices1 index set for first face
         * @param indices2 index set for second face
         * @return true if the index sets are equivalent
         */
        public bool faceIndicesEqual(int[] indices1, int[] indices2)
        {
            if (indices1.Length != indices2.Length)
            {
                return false;
            }
            int len = indices1.Length;
            int j;
            for (j = 0; j < len; j++)
            {
                if (indices1[0] == indices2[j])
                {
                    break;
                }
            }
            if (j == len)
            {
                return false;
            }
            for (int i = 1; i < len; i++)
            {
                if (indices1[i] != indices2[(j + i) % len])
                {
                    return false;
                }
            }
            return true;
        }

        /**
         * Returns the coordinates for <code>num</code> points whose x, y, and
         * z values are randomly chosen within a given range.
         *
         * @param num number of points to produce
         * @param range coordinate values will lie between -range and range
         * @return array of coordinate values
         */
        public double[] randomPoints(int num, double range)
        {
            double[] coords = new double[num * 3];

            for (int i = 0; i < num; i++)
            {
                for (int k = 0; k < 3; k++)
                {
                    coords[i * 3 + k] = 2 * range * (rand.NextDouble() - 0.5);
                }
            }
            return coords;
        }

        private void randomlyPerturb(Vector3 pnt, double tol)
        {
            pnt.X += (float)(tol * (rand.NextDouble() - 0.5));
            pnt.Y += (float)(tol * (rand.NextDouble() - 0.5));
            pnt.Z += (float)(tol * (rand.NextDouble() - 0.5));
        }

        private Vector3 getRandom(float lower, float upper)
        {
            var range = upper - lower;
            return new Vector3
            {
                X = (float)(rand.NextDouble() * range + lower),
                Y = (float)(rand.NextDouble() * range + lower),
                Z = (float)(rand.NextDouble() * range + lower)
            };
        }

        /**
         * Returns the coordinates for <code>num</code> randomly
         * chosen points which are degenerate which respect
         * to the specified dimensionality.
         *
         * @param num number of points to produce
         * @param dimen dimensionality of degeneracy: 0 = coincident,
         * 1 = colinear, 2 = coplaner.
         * @return array of coordinate values
         */
        public double[] randomDegeneratePoints(int num, int dimen)
        {
            double[] coords = new double[num * 3];
            Vector3 pnt = new Vector3();

            Vector3 baseV = getRandom(-1, 1);

            double tol = DOUBLE_PREC;

            if (dimen == 0)
            {
                for (int i = 0; i < num; i++)
                {
                    pnt = baseV;
                    randomlyPerturb(pnt, tol);
                    coords[i * 3 + 0] = pnt.X;
                    coords[i * 3 + 1] = pnt.Y;
                    coords[i * 3 + 2] = pnt.Z;
                }
            }
            else if (dimen == 1)
            {
                Vector3 u = Vector3.Normalize(getRandom(-1, 1));

                for (int i = 0; i < num; i++)
                {
                    double a = 2 * (rand.NextDouble() - 0.5);
                    pnt = Vector3.Multiply((float)a, u);
                    pnt += baseV;
                    randomlyPerturb(pnt, tol);
                    coords[i * 3 + 0] = pnt.X;
                    coords[i * 3 + 1] = pnt.Y;
                    coords[i * 3 + 2] = pnt.Z;
                }
            }
            else // dimen == 2
            {
                Vector3 nrm = Vector3.Normalize(getRandom(-1, 1));

                for (int i = 0; i < num; i++)
                { // compute a random point and project it to the plane
                    Vector3 perp = new Vector3();
                    pnt = getRandom(-1, 1);

                    perp = Vector3.Multiply(Vector3.Dot(pnt, nrm), nrm);
                    pnt -= perp;
                    pnt += baseV;
                    randomlyPerturb(pnt, tol);
                    coords[i * 3 + 0] = pnt.X;
                    coords[i * 3 + 1] = pnt.Y;
                    coords[i * 3 + 2] = pnt.Z;
                }
            }
            return coords;
        }

        /**
         * Returns the coordinates for <code>num</code> points whose x, y, and
         * z values are randomly chosen to lie within a sphere.
         *
         * @param num number of points to produce
         * @param radius radius of the sphere
         * @return array of coordinate values
         */
        public double[] randomSphericalPoints(int num, int radius)
        {
            double[] coords = new double[num * 3];
            Vector3 pnt = new Vector3();

            for (int i = 0; i < num;)
            {
                pnt = new Vector3(rand.Next(-radius, radius), rand.Next(-radius, radius), rand.Next(-radius, radius));
                if (norm(pnt) <= radius)
                {
                    coords[i * 3 + 0] = pnt.X;
                    coords[i * 3 + 1] = pnt.Y;
                    coords[i * 3 + 2] = pnt.Z;
                    i++;
                }
            }
            return coords;
        }

        private double norm(Vector3 v)
        {
            return Math.Sqrt(v.X * v.X + v.Y * v.Y + v.Z * v.Z);
        }

        /**
         * Returns the coordinates for <code>num</code> points whose x, y, and
         * z values are each randomly chosen to lie within a specified
         * range, and then clipped to a maximum absolute
         * value. This means a large number of points
         * may lie on the surface of cube, which is useful
         * for creating degenerate convex hull situations.
         *
         * @param num number of points to produce
         * @param range coordinate values will lie between -range and
         * range, before clipping
         * @param max maximum absolute value to which the coordinates
         * are clipped
         * @return array of coordinate values
         */
        public double[] randomCubedPoints(int num, double range, double max)
        {
            double[] coords = new double[num * 3];

            for (int i = 0; i < num; i++)
            {
                for (int k = 0; k < 3; k++)
                {
                    double x = 2 * range * (rand.NextDouble() - 0.5);
                    if (x > max)
                    {
                        x = max;
                    }
                    else if (x < -max)
                    {
                        x = -max;
                    }
                    coords[i * 3 + k] = x;
                }
            }
            return coords;
        }

        private double[] shuffleCoords(double[] coords)
        {
            int num = coords.Length / 3;

            for (int i = 0; i < num; i++)
            {
                int i1 = rand.Next(num);
                int i2 = rand.Next(num);
                for (int k = 0; k < 3; k++)
                {
                    double tmp = coords[i1 * 3 + k];
                    coords[i1 * 3 + k] = coords[i2 * 3 + k];
                    coords[i2 * 3 + k] = tmp;
                }
            }
            return coords;
        }

        /**
         * Returns randomly shuffled coordinates for points on a
         * three-dimensional grid, with a presecribed width between each point.
         *
         * @param gridSize number of points in each direction,
         * so that the total number of points produced is the cube of
         * gridSize.
         * @param width distance between each point along a particular
         * direction
         * @return array of coordinate values
         */
        public double[] randomGridPoints(int gridSize, double width)
        {
            // gridSize gives the number of points across a given dimension
            // any given coordinate indexed by i has value
            // (i/(gridSize-1) - 0.5)*width

            int num = gridSize * gridSize * gridSize;

            double[] coords = new double[num * 3];

            int idx = 0;
            for (int i = 0; i < gridSize; i++)
            {
                for (int j = 0; j < gridSize; j++)
                {
                    for (int k = 0; k < gridSize; k++)
                    {
                        coords[idx * 3 + 0] = (i / (double)(gridSize - 1) - 0.5) * width;
                        coords[idx * 3 + 1] = (j / (double)(gridSize - 1) - 0.5) * width;
                        coords[idx * 3 + 2] = (k / (double)(gridSize - 1) - 0.5) * width;
                        idx++;
                    }
                }
            }
            shuffleCoords(coords);
            return coords;
        }

        void explicitFaceCheck(QuickHull hull, int[][] checkFaces)
        {
            int[][] faceIndices = hull.GetFaces();
            if (faceIndices.Length != checkFaces.Length)
            {
                throw new Exception("Error: " + faceIndices.Length + " faces vs. " + checkFaces.Length);
            }

            // translate face indices back into original indices
            int[] vtxIndices = hull.GetVertexPointIndices();

            for (int j = 0; j < faceIndices.Length; j++)
            {
                int[] idxs = faceIndices[j];
                for (int k = 0; k < idxs.Length; k++)
                {
                    idxs[k] = vtxIndices[idxs[k]];
                }
            }
            for (int i = 0; i < checkFaces.Length; i++)
            {
                int[] cf = checkFaces[i];
                int j;
                for (j = 0; j < faceIndices.Length; j++)
                {
                    if (faceIndices[j] != null)
                    {
                        if (faceIndicesEqual(cf, faceIndices[j]))
                        {
                            faceIndices[j] = null;
                            break;
                        }
                    }
                }
                if (j == faceIndices.Length)
                {
                    String s = "";
                    for (int k = 0; k < cf.Length; k++)
                    {
                        s += cf[k] + " ";
                    }
                    throw new Exception("Error: face " + s + " not found");
                }
            }
        }

        int cnt = 0;

        void singleTest(double[] coords, int[][] checkFaces)
        {
            QuickHull hull = new QuickHull(coords);

            if (triangulate)
            {
                hull.Triangulate();
            }

            if (checkFaces != null)
            {
                explicitFaceCheck(hull, checkFaces);
            }
            if (degeneracyTest != NO_DEGENERACY)
            {
                degenerateTest(hull, coords);
            }
        }

        double[] addDegeneracy(int type, double[] coords, QuickHull hull)
        {
            int numv = coords.Length / 3;
            int[][] faces = hull.GetFaces();
            double[] coordsx = new double[coords.Length + faces.Length * 3];
            for (int i = 0; i < coords.Length; i++)
            {
                coordsx[i] = coords[i];
            }

            double[] lam = new double[3];
            double eps = hull.Tolerance;

            for (int i = 0; i < faces.Length; i++)
            {
                // random point on an edge
                lam[0] = rand.NextDouble();
                lam[1] = 1 - lam[0];
                lam[2] = 0.0;

                if (type == VERTEX_DEGENERACY && (i % 2 == 0))
                {
                    lam[0] = 1.0;
                    lam[1] = lam[2] = 0;
                }

                for (int j = 0; j < 3; j++)
                {
                    int vtxi = faces[i][j];
                    for (int k = 0; k < 3; k++)
                    {
                        coordsx[numv * 3 + k] +=
                           lam[j] * coords[vtxi * 3 + k] +
                           epsScale * eps * (rand.NextDouble() - 0.5);
                    }
                }
                numv++;
            }
            shuffleCoords(coordsx);
            return coordsx;
        }

        void degenerateTest(QuickHull hull, double[] coords)
        {
            double[] coordsx = addDegeneracy(degeneracyTest, coords, hull);

            QuickHull xhull = new QuickHull(coordsx);

            if (triangulate)
            {
                xhull.Triangulate();
            }
        }

        void rotateCoords(double[] res, double[] xyz, double roll, double pitch, double yaw)
        {
            double sroll = Math.Sin(roll);
            double croll = Math.Cos(roll);
            double spitch = Math.Sin(pitch);
            double cpitch = Math.Cos(pitch);
            double syaw = Math.Sin(yaw);
            double cyaw = Math.Cos(yaw);

            double m00 = croll * cpitch;
            double m10 = sroll * cpitch;
            double m20 = -spitch;

            double m01 = croll * spitch * syaw - sroll * cyaw;
            double m11 = sroll * spitch * syaw + croll * cyaw;
            double m21 = cpitch * syaw;

            double m02 = croll * spitch * cyaw + sroll * syaw;
            double m12 = sroll * spitch * cyaw - croll * syaw;
            double m22 = cpitch * cyaw;

            double x, y, z;

            for (int i = 0; i < xyz.Length - 2; i += 3)
            {
                res[i + 0] = m00 * xyz[i + 0] + m01 * xyz[i + 1] + m02 * xyz[i + 2];
                res[i + 1] = m10 * xyz[i + 0] + m11 * xyz[i + 1] + m12 * xyz[i + 2];
                res[i + 2] = m20 * xyz[i + 0] + m21 * xyz[i + 1] + m22 * xyz[i + 2];
            }
        }

        void printCoords(double[] coords)
        {
            int nump = coords.Length / 3;
            for (int i = 0; i < nump; i++)
            {
                this.output.WriteLine(
                    coords[i * 3 + 0] + ", " +
                    coords[i * 3 + 1] + ", " +
                    coords[i * 3 + 2] + ", ");
            }
        }

        void testException(double[] coords, string msg)
        {
            QuickHull hull;
            Exception ex = null;
            try
            {
                hull = new QuickHull(coords);
            }
            catch (Exception e)
            {
                ex = e;
            }
            if (ex == null)
            {
                this.output.WriteLine("Expected exception " + msg);
                this.output.WriteLine("Got no exception");
                this.output.WriteLine("Input pnts:");
                printCoords(coords);
                Assert.NotNull(ex);
            }
            else if (ex.Message == null ||
                 !ex.Message.Equals(msg))
            {
                this.output.WriteLine("Expected exception " + msg);
                this.output.WriteLine("Got exception " + ex.Message);
                this.output.WriteLine("Input pnts:");
                printCoords(coords);
                Assert.False(true, "Expected exception " + msg + " \r\nGot exception " + ex.Message);
            }
        }

        void test(double[] coords, int[][] checkFaces)
        {

            double[][] rpyList = new double[][]
             {
                  new double[]{  0,  0,  0},
                  new double[]{ 10, 20, 30},
                  new double[]{ -45, 60, 91},
                  new double[]{ 125, 67, 81}
             };
            double[] xcoords = new double[coords.Length];

            singleTest(coords, checkFaces);
            if (testRotation)
            {
                for (int i = 0; i < rpyList.Length; i++)
                {
                    double[] rpy = rpyList[i];
                    rotateCoords(xcoords, coords,
                              MathExt.ToRadians(rpy[0]),
                              MathExt.ToRadians(rpy[1]),
                              MathExt.ToRadians(rpy[2]));
                    singleTest(xcoords, checkFaces);
                }
            }
        }
    }
}
