using OpenH2.Core.Architecture;
using OpenH2.Core.Extensions;
using OpenH2.Core.Representations;
using OpenH2.Core.Tags;
using OpenH2.Engine.Components;
using OpenH2.Engine.Entities;
using OpenH2.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace OpenH2.Engine.EntityFactories
{
    public class BlocFactory
    {
        public static Scenery FromTag(H2vMap map, ScenarioTag scenario,  ScenarioTag.BlocInstance instance)
        {
            var scenery = new Scenery();

            var id = scenario.BlocDefinitions[instance.SceneryDefinitionIndex].BlocId;
            map.TryGetTag<BlocTag>(id, out var tag);

            if(map.TryGetTag<PhysicalModelTag>(tag.HlmtId, out var hlmt) == false)
            {
                throw new Exception("No model found for bloc");
            }

            if (map.TryGetTag<ModelTag>(hlmt.ModelId, out var model) == false)
            {
                Console.WriteLine($"No MODE[{hlmt.ModelId}] found for HLMT[{hlmt.Id}]");
                return scenery;
            }

            var meshes = new List<Mesh>();

            foreach(var part in model.Parts)
            {
                meshes.AddRange(part.Model.Meshes);
            }

            var comp = new RenderModelComponent(scenery);
            comp.Note = $"[{tag.Id}] {tag.Name}";
            comp.Meshes = meshes.ToArray();
            comp.Position = new Vector3(instance.X, instance.Y, instance.Z);
            comp.Orientation = new Vector3(instance.Roll, instance.Yaw, instance.Pitch);
            comp.Scale = new Vector3(tag.UniformScale);

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

            var components = new List<Component>();
            components.Add(comp);
            scenery.SetComponents(components.ToArray());

            return scenery;
        }
    }
}
