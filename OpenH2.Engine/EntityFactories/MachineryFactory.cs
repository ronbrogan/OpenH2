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
    public class MachineryFactory
    {
        public static Scenery FromTag(H2vMap map, ScenarioTag scenario,  ScenarioTag.MachineryInstance instance)
        {
            var scenery = new Scenery();

            var id = scenario.MachineryDefinitions[instance.MachineryDefinitionIndex].Machinery;
            map.TryGetTag(id, out var tag);

            if(tag.PhysicalModel == uint.MaxValue)
            {
                Console.WriteLine($"No HLMT specified in MACH:{tag.Name}");
                return scenery;
            }

            if(map.TryGetTag(tag.PhysicalModel, out var hlmt) == false)
            {
                throw new Exception("No model found for MACH");
            }

            if (map.TryGetTag(hlmt.Model, out var model) == false)
            {
                Console.WriteLine($"No MODE[{hlmt.Model}] found for HLMT[{hlmt.Id}]");
                return scenery;
            }

            var meshes = new List<Mesh>();

            var partIndex = model.Lods.First().Permutations.First().HighestPieceIndex;
            meshes.AddRange(model.Parts[partIndex].Model.Meshes);
            

            var comp = new RenderModelComponent(scenery);
            comp.Note = $"[{tag.Id}] {tag.Name}";
            comp.Meshes = meshes.ToArray();
            comp.Position = instance.Position;
            comp.Orientation = instance.Orientation.ToQuaternion();
            comp.Scale = new Vector3(1);

            foreach (var mesh in comp.Meshes)
            {
                if (comp.Materials.ContainsKey(mesh.MaterialIdentifier))
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

            var components = new List<Component>();
            components.Add(comp);
            scenery.SetComponents(components.ToArray());

            return scenery;
        }
    }
}
