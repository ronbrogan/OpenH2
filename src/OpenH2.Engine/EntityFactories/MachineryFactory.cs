using OpenH2.Core.Architecture;
using OpenH2.Core.Extensions;
using OpenH2.Core.Representations;
using OpenH2.Core.Tags;
using OpenH2.Core.Tags.Common;
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

            if(instance.MachineryDefinitionIndex == ushort.MaxValue)
            {
                Console.WriteLine($"MACH index out of range");
                return scenery;
            }

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

            var meshes = new List<ModelMesh>();

            var partIndex = model.Lods.First().Permutations.First().HighestPieceIndex;
            meshes.AddRange(model.Parts[partIndex].Model.Meshes);

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

            var comp = new RenderModelComponent(scenery)
            {
                RenderModel = new Model<BitmapTag>
                {
                    Note = $"[{tag.Id}] {tag.Name}",
                    Meshes = renderModelMeshes.ToArray(),
                    Flags = ModelFlags.Diffuse | ModelFlags.CastsShadows | ModelFlags.ReceivesShadows
                }
            };

            var xform = new TransformComponent(scenery)
            {
                Position = instance.Position,
                Orientation = Quaternion.CreateFromYawPitchRoll(instance.Orientation.Y, instance.Orientation.Z, instance.Orientation.X)
            };

            scenery.SetComponents(new Component[] { comp, xform });

            return scenery;
        }
    }
}
