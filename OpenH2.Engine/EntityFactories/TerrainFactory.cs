using OpenH2.Core.Architecture;
using OpenH2.Core.Extensions;
using OpenH2.Core.Representations;
using OpenH2.Core.Tags;
using OpenH2.Engine.Components;
using OpenH2.Engine.Entities;
using OpenH2.Foundation;
using OpenH2.Translation.TagData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenH2.Engine.EntityFactories
{
    public static class TerrainFactory
    {
        public static Terrain FromBspData(H2vMap map, BspTagData bspData)
        {
            var tag = (Bsp)map.Tags[bspData.Id];

            var terrain = new Terrain();

            var components = new List<Component>();

            var comp = new RenderModelComponent(terrain);

            foreach (var model in bspData.RenderModels)
            {
                comp.Meshes.AddRange(model.Meshes);
            }

            foreach(var mesh in comp.Meshes)
            {
                if(comp.Materials.ContainsKey(mesh.MaterialIdentifier))
                {
                    continue;
                }

                var mat = new Material<Bitmap>();
                mat.DiffuseColor = VectorExtensions.RandomColor();
                comp.Materials.Add(mesh.MaterialIdentifier, mat);

                if(tag.Shaders.Length <= mesh.MaterialIdentifier)
                {
                    continue;
                }

                var shaderRef = tag.Shaders[mesh.MaterialIdentifier];
                Shader shader;
                if (map.Tags.TryGetValue(shaderRef.ShaderId, out var shaderTag) == false)
                {
                    continue;
                }

                shader = (Shader)shaderTag;

                if (shader.BitmapInfos.Length > 0)
                {
                    var bitms = shader.BitmapInfos[0];

                    mat.DiffuseMap = map.Tags.GetValueOrDefault(bitms.DiffuseBitmapId) as Bitmap;

                    var bitmRefs = shader.Parameters.SelectMany(p => p.BitmapParameter1s.Select(b => b.BitmapId));

                    foreach (var bitmRef in bitmRefs)
                    {
                        var bitm = map.Tags.GetValueOrDefault(bitmRef) as Bitmap;

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
