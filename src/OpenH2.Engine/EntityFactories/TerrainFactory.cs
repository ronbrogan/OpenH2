using OpenH2.Core.Architecture;
using OpenH2.Core.Extensions;
using OpenH2.Core.Representations;
using OpenH2.Core.Tags;
using OpenH2.Core.Tags.Common;
using OpenH2.Engine.Components;
using OpenH2.Engine.Entities;
using OpenH2.Foundation;
using System.Collections.Generic;

namespace OpenH2.Engine.EntityFactories
{
    public static class TerrainFactory
    {
        public static Terrain FromBspData(H2vMap map, BspTag tag)
        {
            var terrain = new Terrain();

            var components = new List<Component>();

            var meshes = new List<ModelMesh>();

            foreach (var chunk in tag.RenderChunks)
            {
                meshes.AddRange(chunk.Model.Meshes);
            }

            var renderModelMeshes = new List<Mesh<BitmapTag>>(meshes.Count);

            foreach (var mesh in meshes)
            {
                var mat = new Material<BitmapTag>();
                mat.DiffuseColor = VectorExtensions.RandomColor();

                if (map.TryGetTag(mesh.Shader, out var shader))
                {
                    MaterialFactory.PopulateMaterial(map, mat, shader);
                }

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

            var comp = new RenderModelComponent(terrain)
            {
                RenderModel = new Model<BitmapTag>
                {
                    Meshes = renderModelMeshes.ToArray(),
                    Flags = ModelFlags.Diffuse | ModelFlags.ReceivesShadows | ModelFlags.IsStatic
                }
            };

            var xform = new TransformComponent(terrain);

            terrain.SetComponents(new Component[] { comp, xform });

            return terrain;
        }
    }
}
