using OpenH2.Core.Architecture;
using OpenH2.Core.Extensions;
using OpenH2.Core.Representations;
using OpenH2.Core.Tags;
using OpenH2.Core.Tags.Common;
using OpenH2.Engine.Components;
using OpenH2.Engine.Entities;
using OpenH2.Foundation;
using OpenH2.Physics.Colliders;
using OpenH2.Physics.SpatialPartitioning;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace OpenH2.Engine.EntityFactories
{
    public static class TerrainFactory
    {
        public static Terrain FromBspData(H2vMap map, BspTag tag)
        {
            var terrain = new Terrain();

            var meshes = new List<ModelMesh>();

            foreach (var chunk in tag.RenderChunks)
            {
                meshes.AddRange(chunk.Model.Meshes);
            }

            var renderModelMeshes = new List<Mesh<BitmapTag>>(meshes.Count);

            foreach (var mesh in meshes)
            {
                var mat = map.CreateMaterial(mesh);

                renderModelMeshes.Add(new Mesh<BitmapTag>()
                {
                    Compressed = mesh.Compressed,
                    ElementType = mesh.ElementType,
                    Indicies = mesh.Indices,
                    Note = mesh.Note,
                    RawData = mesh.RawData,
                    Verticies = mesh.Verticies,

                    Material = mat
                });
            }

            var renderModel = new RenderModelComponent(terrain)
            {
                RenderModel = new Model<BitmapTag>
                {
                    Meshes = renderModelMeshes.ToArray(),
                    Flags = ModelFlags.Diffuse | ModelFlags.ReceivesShadows | ModelFlags.IsStatic
                }
            };

            var collisionTerrain = new StaticTerrainComponent(terrain)
            {
                Collision = GetFaces(tag).ToArray()
            };

            var xform = new TransformComponent(terrain, Vector3.Zero);

            terrain.SetComponents(new Component[] { renderModel, xform, collisionTerrain });

            return terrain;
        }

        public static List<FaceCollider> GetFaces(BspTag tag)
        {
            var totalFaces = tag.CollisionInfos.Sum(c => c.Faces.Length);
            var root = new List<FaceCollider>(totalFaces);

            foreach (var col in tag.CollisionInfos)
            {
                for (var faceIndex = 0; faceIndex < col.Faces.Length; faceIndex++)
                {
                    var face = col.Faces[faceIndex];
                    var verts = new List<ushort>(8);

                    ushort edgeIndex = face.FirstEdge;
                    do
                    {
                        var edge = col.HalfEdges[edgeIndex];

                        verts.Add(edge.Face0 == faceIndex
                            ? edge.Vertex0
                            : edge.Vertex1);

                        edgeIndex = edge.Face0 == faceIndex
                            ? edge.NextEdge
                            : edge.PrevEdge;

                    } while (edgeIndex != face.FirstEdge);

                    var faceVerts = new Vector3[verts.Count];

                    Vector3 min = new Vector3(float.MaxValue);
                    Vector3 max = new Vector3(float.MinValue);

                    for (int i = 0; i < verts.Count; i++)
                    {
                        ushort index = (ushort)verts[i];
                        var vert = col.Verticies[index];
                        faceVerts[i] = new Vector3(vert.x, vert.y, vert.z);

                        if (vert.x < min.X) min.X = vert.x;
                        if (vert.y < min.Y) min.Y = vert.y;
                        if (vert.z < min.Z) min.Z = vert.z;

                        if (vert.x > max.X) max.X = vert.x;
                        if (vert.y > max.Y) max.Y = vert.y;
                        if (vert.z > max.Z) max.Z = vert.z;
                    }

                    var newFace = new Face(faceVerts);
                    var faceCollider = new FaceCollider(newFace, min, max);

                    root.Add(faceCollider);
                }
            }

            return root;
        }
    }
}
