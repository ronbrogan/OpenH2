using OpenH2.Core.Architecture;
using OpenH2.Core.Maps.Vista;
using OpenH2.Core.Tags;
using OpenH2.Core.Tags.Common.Models;
using OpenH2.Engine.Components;
using OpenH2.Engine.Entities;
using OpenH2.Engine.Factories;
using OpenH2.Foundation;
using System.Collections.Generic;
using System.Numerics;

namespace OpenH2.Engine.EntityFactories
{
    public static class TerrainFactory
    {
        public static Terrain FromBspData(H2vMap map, BspTag tag)
        {
            var terrain = new Terrain();
            terrain.FriendlyName = tag.Name;

            var meshes = new List<ModelMesh>();

            foreach (var chunk in tag.RenderChunks)
            {
                meshes.AddRange(chunk.Model.Meshes);
            }

            var transparentMeshes = new List<Mesh<BitmapTag>>(meshes.Count);
            var renderModelMeshes = new List<Mesh<BitmapTag>>(meshes.Count);

            foreach (var mesh in meshes)
            {
                var mat = map.CreateMaterial(mesh);

                var renderMesh = new Mesh<BitmapTag>()
                {
                    Compressed = mesh.Compressed,
                    ElementType = mesh.ElementType,
                    Indicies = mesh.Indices,
                    Note = mesh.Note,
                    RawData = mesh.RawData,
                    Verticies = mesh.Verticies,

                    Material = mat
                };

                if (mat.AlphaMap == null)
                {
                    renderModelMeshes.Add(renderMesh);
                }
                else
                {
                    transparentMeshes.Add(renderMesh);
                }

            }

            var components = new List<Component>();

            components.Add(new RenderModelComponent(terrain, new Model<BitmapTag>
            {
                Meshes = renderModelMeshes.ToArray(),
                Flags = ModelFlags.Diffuse | ModelFlags.ReceivesShadows | ModelFlags.IsStatic
            }));

            foreach(var mesh in transparentMeshes)
            {
                components.Add(new RenderModelComponent(terrain, new Model<BitmapTag>
                {
                    Meshes = new[] { mesh },
                    Flags = ModelFlags.IsTransparent | ModelFlags.IsStatic
                }));
            }

            var collisionTerrain = PhysicsComponentFactory.CreateTerrain(terrain, tag.CollisionInfos, tag.Shaders);
            components.Add(collisionTerrain);

            components.Add(new RenderModelComponent(terrain, new Model<BitmapTag>
            {
                Meshes = MeshFactory.GetRenderModel(collisionTerrain.Collider),
                Flags = ModelFlags.Wireframe | ModelFlags.IsStatic,
                RenderLayer = RenderLayers.Collision
            }));

            components.Add(new TransformComponent(terrain, Vector3.Zero));

            terrain.SetComponents(components);

            return terrain;
        }
    }
}
