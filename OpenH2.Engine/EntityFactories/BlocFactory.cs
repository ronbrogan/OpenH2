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

namespace OpenH2.Engine.EntityFactories
{
    public class BlocFactory
    {
        public static Scenery FromTag(H2vMap map, ScenarioTag scenario,  ScenarioTag.BlocInstance instance)
        {
            var scenery = new Scenery();

            var bloc = scenario.BlocDefinitions[instance.BlocDefinitionIndex].Bloc;
            map.TryGetTag(bloc, out var tag);

            if(map.TryGetTag(tag.PhysicalModel, out var hlmt) == false)
            {
                throw new Exception("No model found for bloc");
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
                    Indicies = mesh.Indicies,
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
                    Position = instance.Position,
                    Orientation = instance.Orientation.ToQuaternion(),
                    Flags = ModelFlags.Diffuse | ModelFlags.CastsShadows | ModelFlags.ReceivesShadows,
                    Meshes = renderModelMeshes.ToArray()
                }
            };

            var components = new List<Component>();
            components.Add(comp);
            scenery.SetComponents(components.ToArray());

            return scenery;
        }
    }
}
