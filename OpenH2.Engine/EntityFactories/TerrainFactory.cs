using OpenH2.Core.Architecture;
using OpenH2.Core.Extensions;
using OpenH2.Core.Representations;
using OpenH2.Core.Tags;
using OpenH2.Engine.Components;
using OpenH2.Engine.Entities;
using OpenH2.Foundation;
using OpenH2.Translation.TagData;
using System.Collections.Generic;
using System.Linq;

namespace OpenH2.Engine.EntityFactories
{
    public static class TerrainFactory
    {
        public static Terrain FromBspData(H2vMap map, BspTagData bspData)
        {
            var tag = (BspTag)map.Tags[bspData.Id];

            var terrain = new Terrain();

            var components = new List<Component>();

            var meshes = new List<Mesh>();

            foreach (var model in bspData.RenderModels)
            {
                meshes.AddRange(model.Meshes);
            }

            var comp = new RenderModelComponent(terrain);
            comp.Meshes = meshes.ToArray();

            foreach (var mesh in comp.Meshes)
            {
                if(comp.Materials.ContainsKey(mesh.MaterialIdentifier))
                {
                    continue;
                }

                var mat = new Material<BitmapTag>();
                mat.DiffuseColor = VectorExtensions.RandomColor();
                comp.Materials.Add(mesh.MaterialIdentifier, mat);

                ShaderTag shader;
                if (map.Tags.TryGetValue(mesh.MaterialIdentifier, out var shaderTag) == false)
                {
                    continue;
                }

                shader = (ShaderTag)shaderTag;

                if (shader.BitmapInfos.Length > 0)
                {
                    var bitms = shader.BitmapInfos[0];

                    mat.DiffuseMap = map.Tags.GetValueOrDefault(bitms.DiffuseBitmapId) as BitmapTag;

                    var bitmRefs = shader.Parameters.SelectMany(p => p.BitmapParameter1s.Select(b => b.BitmapId));

                    foreach (var bitmRef in bitmRefs)
                    {
                        var bitm = map.Tags.GetValueOrDefault(bitmRef) as BitmapTag;

                        if (bitm != null && bitm.TextureUsage == Core.Enums.Texture.TextureUsage.Bump)
                        {
                            mat.NormalMap = bitm;
                        }
                    }
                }
            }

            components.Add(comp);
            components.Add(new TransformComponent(terrain));

            terrain.SetComponents(components.ToArray());

            return terrain;
        }
    }
}
