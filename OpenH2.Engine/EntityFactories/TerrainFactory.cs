using OpenH2.Core.Architecture;
using OpenH2.Core.Extensions;
using OpenH2.Core.Representations;
using OpenH2.Core.Tags;
using OpenH2.Engine.Components;
using OpenH2.Engine.Entities;
using OpenH2.Foundation;
using System.Collections.Generic;
using System.Linq;

namespace OpenH2.Engine.EntityFactories
{
    public static class TerrainFactory
    {
        public static Terrain FromBspData(H2vMap map, BspTag tag)
        {
            var terrain = new Terrain();

            var components = new List<Component>();

            var meshes = new List<Mesh>();

            foreach (var chunk in tag.RenderChunks)
            {
                meshes.AddRange(chunk.Model.Meshes);
            }

            var comp = new RenderModelComponent(terrain)
            {
                Meshes = meshes.ToArray(),
                Flags = ModelFlags.Diffuse | ModelFlags.ReceivesShadows | ModelFlags.IsStatic
            };

            foreach (var mesh in comp.Meshes)
            {
                if(comp.Materials.ContainsKey(mesh.MaterialIdentifier))
                {
                    continue;
                }

                var mat = new Material<BitmapTag>();
                mat.DiffuseColor = VectorExtensions.RandomColor();
                comp.Materials.Add(mesh.MaterialIdentifier, mat);

                if (map.TryGetTag<ShaderTag>(mesh.MaterialIdentifier, out var shader))
                {
                    MaterialFactory.PopulateMaterial(map, mat, shader);
                }
            }

            components.Add(comp);
            components.Add(new TransformComponent(terrain));

            terrain.SetComponents(components.ToArray());

            return terrain;
        }
    }
}
