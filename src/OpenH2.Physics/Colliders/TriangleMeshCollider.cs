using OpenH2.Core.Tags.Common.Collision;
using OpenH2.Foundation.Physics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace OpenH2.Physics.Colliders
{
    public class TriangleMeshCollider : IVertexBasedCollider
    {
        public int PhysicsMaterial => -1;

        public Vector3[] Vertices { get; set; }

        /// <summary>
        /// Array of vertex indices, where each three items constitutes a single triangle
        /// </summary>
        public int[] TriangleIndices { get; set; }

        /// <summary>
        /// Array of material indices, where each item is the given triangles global material index
        /// </summary>
        public int[] MaterialIndices { get; set; }

        public Vector3[] GetTransformedVertices() => Vertices;

        public static TriangleMeshCollider Create(ICollisionInfo[] collisionInfos, Func<int, int> materialLookup)
        {
            var totalFaces = collisionInfos.Sum(c => c.Faces.Length);

            List<Vector3> verts = new List<Vector3>(collisionInfos.Sum(c => c.Vertices.Length));
            List<int> indices = new List<int>(totalFaces * 4);
            List<int> matIndices = new List<int>(totalFaces);

            for (var i = 0; i < collisionInfos.Length; i++)
            {
                var currentVertStart = verts.Count();

                var col = collisionInfos[i];
                verts.AddRange(col.Vertices.Select(v => new Vector3(v.x, v.y, v.z)));

                var faceVerts = new List<ushort>(8);

                for (var faceIndex = 0; faceIndex < col.Faces.Length; faceIndex++)
                {
                    var face = col.Faces[faceIndex];

                    ushort edgeIndex = face.FirstEdge;
                    do
                    {
                        var edge = col.HalfEdges[edgeIndex];

                        faceVerts.Add(edge.Face0 == faceIndex
                            ? edge.Vertex0
                            : edge.Vertex1);

                        edgeIndex = edge.Face0 == faceIndex
                            ? edge.NextEdge
                            : edge.PrevEdge;

                    } while (edgeIndex != face.FirstEdge);

                    // Triangulate into a fan, this assumes that we're working with convex
                    // polygons with no colinear triplets, if this isn't sufficient we'll
                    // have to resort to ear cutting
                    var possibleTriangles = faceVerts.Count - 2;
                    var first = faceVerts[0];
                    for (var f = 0; f < possibleTriangles; f++)
                    {
                        var second = faceVerts[f + 1];
                        var third = faceVerts[f + 2];

                        indices.Add(currentVertStart + first);
                        indices.Add(currentVertStart + second);
                        indices.Add(currentVertStart + third);

                        matIndices.Add(materialLookup(face.ShaderIndex));
                    }

                    faceVerts.Clear();
                }
            }

            return new TriangleMeshCollider()
            {
                Vertices = verts.ToArray(),
                TriangleIndices = indices.ToArray(),
                MaterialIndices = matIndices.ToArray()
            };
        }
    }
}
