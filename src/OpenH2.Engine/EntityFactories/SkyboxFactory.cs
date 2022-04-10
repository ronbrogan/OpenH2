using OpenH2.Core.Architecture;
using OpenH2.Core.Maps.Vista;
using OpenH2.Core.Tags;
using OpenH2.Core.Tags.Common.Models;
using OpenH2.Core.Tags.Scenario;
using OpenH2.Engine.Components;
using OpenH2.Engine.Entities;
using OpenH2.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace OpenH2.Engine.EntityFactories
{
    public class SkyboxFactory
    {
        public static Scenery FromTag(H2vMap map, ScenarioTag scenario,  ScenarioTag.SkyboxInstance instance)
        {
            var scenery = new Scenery();
            scenery.FriendlyName = "Skybox_" + instance.Skybox.Id;

            var tag = map.GetTag(instance.Skybox);

            if (map.TryGetTag(tag.Model, out var model) == false)
            {
                Console.WriteLine($"No MODE[{tag.Model}] found for SKY[{instance.Skybox}]");
                return scenery;
            }

            var meshes = new List<ModelMesh>();

            var partIndex = model.Regions.First().Permutations.First().HighestPieceIndex;
            meshes.AddRange(model.Sections[partIndex].Model.Meshes);

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

            var comp = new RenderModelComponent(scenery, new Model<BitmapTag>
            {
                Note = $"[{tag.Id}] {tag.Name}",
                Meshes = renderModelMeshes.ToArray(),
                Flags = ModelFlags.IsSkybox
            });

            var light = new SkyLightComponent(scenery)
            {
                Direction = new Vector3(7, 7, -20)
            };

            var components = new List<Component>();
            components.Add(comp);
            components.Add(light);
            scenery.SetComponents(components.ToArray());

            return scenery;
        }
    }
}
