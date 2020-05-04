using OpenH2.Core.Architecture;
using OpenH2.Core.Representations;
using OpenH2.Core.Tags;
using OpenH2.Core.Tags.Common.Collision;
using OpenH2.Engine.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static OpenH2.Core.Tags.BspTag;

namespace OpenH2.Engine.Factories
{
    public static class PhysicsComponentFactory
    {
        public static RigidBodyComponent CreateRigidBody(Entity parent, TransformComponent transform, H2vMap map, TagRef<HaloModelTag> hlmtRef, int damageLevel = 0)
        {
            if(map.TryGetTag(hlmtRef, out var hlmt) == false)
            {
                throw new Exception($"Couldn't find HLMT[{hlmtRef.Id}]");
            }

            RigidBodyComponent body;


            if (map.TryGetTag(hlmt.PhysicsModel, out var phmo) && phmo.BodyParameters.Length > 0)
            {
                var param = phmo.BodyParameters[0];

                body = new RigidBodyComponent(parent, transform, param.InertiaTensor, param.Mass, param.CenterOfMass);
                body.Collider = ColliderFactory.GetColliderForHlmt(map, hlmt, damageLevel);
            }
            else
            {
                body = new RigidBodyComponent(parent, transform);
            }            
            
            return body;
        }

        public static StaticTerrainComponent CreateTerrain(Entity parent, ICollisionInfo[] collisionInfos, ShaderInfo[] shaders)
        {
            var (verts, indices, matIndices) = GetTriangulatedCollisionMesh(collisionInfos, shaders);

            return new StaticTerrainComponent(parent)
            {
                Vertices = verts.ToArray(),
                TriangleIndices = indices.ToArray(),
                MaterialIndices = matIndices.ToArray()
            };
        }

        public static StaticGeometryComponent CreateStaticGeometry(Entity parent, TransformComponent xform, ICollisionInfo collisionInfos, ShaderInfo[] shaders)
        {
            return CreateStaticGeometry(parent, xform, new[] { collisionInfos }, shaders);
        }

        public static StaticGeometryComponent CreateStaticGeometry(Entity parent, TransformComponent xform, ICollisionInfo[] collisionInfos, ShaderInfo[] shaders)
        {
            var (verts, indices, matIndices) = GetTriangulatedCollisionMesh(collisionInfos, shaders);

            return new StaticGeometryComponent(parent, xform)
            {
                Vertices = verts.ToArray(),
                TriangleIndices = indices.ToArray(),
                MaterialIndices = matIndices.ToArray()
            };
        }

        private static (List<Vector3>,List<int>,List<int>) GetTriangulatedCollisionMesh(ICollisionInfo[] collisionInfos, ShaderInfo[] shaders)
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

                        if(face.ShaderIndex < shaders.Length)
                        {
                            var shader = shaders[face.ShaderIndex];
                            matIndices.Add(shader.GlobalMaterialId);
                        }
                        else
                        {
                            matIndices.Add(-1);
                        }
                    }

                    faceVerts.Clear();
                }
            }

            return (verts, indices, matIndices);
        }
    }
}
