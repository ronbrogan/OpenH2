using OpenH2.Core.Architecture;
using OpenH2.Core.Extensions;
using OpenH2.Core.Representations;
using OpenH2.Core.Tags;
using OpenH2.Core.Tags.Common;
using OpenH2.Core.Tags.Scenario;
using OpenH2.Engine.Components;
using OpenH2.Engine.Entities;
using OpenH2.Engine.Factories;
using OpenH2.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace OpenH2.Engine.EntityFactories
{
    public class SceneryFactory
    {
        public static Scenery FromInstancedGeometry(H2vMap map, BspTag bsp, BspTag.InstancedGeometryInstance instance)
        {
            var scenery = new Scenery();

            if (instance.Index >= bsp.InstancedGeometryDefinitions.Length)
                return scenery;

            var def = bsp.InstancedGeometryDefinitions[instance.Index];

            var renderModelMeshes = new List<Mesh<BitmapTag>>(def.Model.Meshes.Length);

            foreach (var mesh in def.Model.Meshes)
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
                    Note = $"[{bsp.Id}] {bsp.Name}//instanced//{instance.Index}",
                    Meshes = renderModelMeshes.ToArray(),                   
                    Flags = ModelFlags.Diffuse | ModelFlags.CastsShadows | ModelFlags.ReceivesShadows
                }
            };

            var xform = new TransformComponent(scenery, instance.Position, QuaternionExtensions.From3x3Mat(instance.RotationMatrix))
            {
                Scale = new Vector3(instance.Scale),
            };

            xform.UpdateDerivedData();

            scenery.SetComponents(new Component[] { comp, xform });

            return scenery;
        }

        public static Scenery FromTag(H2vMap map, ScenarioTag scenario,  ScenarioTag.SceneryInstance instance)
        {
            var scenery = new Scenery();

            var id = scenario.SceneryDefinitions[instance.SceneryDefinitionIndex].Scenery;
            map.TryGetTag(id, out var tag);

            var comp = new RenderModelComponent(scenery)
            {
                RenderModel = new Model<BitmapTag>
                {
                    Note = $"[{tag.Id}] {tag.Name}",
                    Meshes = MeshFactory.GetModelForHlmt(map, tag.PhysicalModel),
                    Scale = new Vector3(1),
                    Flags = ModelFlags.Diffuse | ModelFlags.CastsShadows | ModelFlags.ReceivesShadows
                }
            };

            var orientation = Quaternion.CreateFromYawPitchRoll(instance.Orientation.Y, instance.Orientation.Z, instance.Orientation.X);
            var xform = new TransformComponent(scenery, instance.Position, orientation);

            scenery.SetComponents(new Component[] { comp, xform });

            return scenery;
        }
    }
}
