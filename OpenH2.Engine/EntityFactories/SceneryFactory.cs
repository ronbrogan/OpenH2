using OpenH2.Core.Architecture;
using OpenH2.Core.Extensions;
using OpenH2.Core.Representations;
using OpenH2.Core.Tags;
using OpenH2.Engine.Components;
using OpenH2.Engine.Entities;
using OpenH2.Foundation;
using OpenH2.Translation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenH2.Engine.EntityFactories
{
    public class SceneryFactory
    {
        public static Scenery FromTag(H2vMap map, SceneryTag tag)
        {
            var scenery = new Scenery();

            if(map.TryGetTag<PhysicalModelTag>(tag.HlmtId, out var hlmt) == false)
            {
                throw new Exception("No model found for scenery");
            }

            if (map.TryGetTag<ModelTag>(hlmt.ModelId, out var model) == false)
            {
                Console.WriteLine($"No MODE[{hlmt.ModelId}] found for HLMT[{hlmt.Id}]");
                return scenery;
            }

            var components = new List<Component>();

            var meshes = new List<Mesh>();

            foreach(var part in model.Parts)
            {
                meshes.AddRange(part.Model.Meshes);
            }

            var comp = new RenderModelComponent(scenery);
            comp.Meshes = meshes.ToArray();
            comp.Position = VectorExtensions.RandomColor() * 20;

            foreach (var mesh in comp.Meshes)
            {
                if (comp.Materials.ContainsKey(mesh.MaterialIdentifier))
                {
                    continue;
                }

                var mat = new Material<BitmapTag>();
                mat.DiffuseColor = VectorExtensions.RandomColor();
                comp.Materials.Add(mesh.MaterialIdentifier, mat);


                if (map.TryGetTag<ShaderTag>(mesh.MaterialIdentifier, out var shader) == false)
                {
                    continue;
                }

                if (shader.BitmapInfos.Length > 0)
                {
                    var bitms = shader.BitmapInfos[0];

                    if (bitms == null)
                        continue;

                    map.TryGetTag<BitmapTag>(bitms.DiffuseBitmapId, out var diffuse);

                    mat.DiffuseMap = diffuse;

                    var bitmRefs = shader.Parameters.SelectMany(p => p.BitmapParameter1s.Select(b => b.BitmapId));
                    foreach (var bitmRef in bitmRefs)
                    {
                        if (map.TryGetTag<BitmapTag>(bitmRef, out var bitm) && bitm.TextureUsage == Core.Enums.Texture.TextureUsage.Bump)
                        {
                            mat.NormalMap = bitm;
                        }
                    }
                }
            }

            components.Add(comp);


            scenery.SetComponents(components.ToArray());

            return scenery;
        }
    }
}
