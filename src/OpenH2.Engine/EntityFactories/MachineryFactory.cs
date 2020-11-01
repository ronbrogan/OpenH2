using OpenH2.Core.Architecture;
using OpenH2.Core.Extensions;
using OpenH2.Core.Maps;
using OpenH2.Core.Tags;
using OpenH2.Core.Tags.Scenario;
using OpenH2.Engine.Components;
using OpenH2.Engine.Entities;
using OpenH2.Engine.Factories;
using OpenH2.Foundation;
using System;
using System.Collections.Generic;

namespace OpenH2.Engine.EntityFactories
{
    public class MachineryFactory
    {
        public static Machine FromTag(H2vMap map, ScenarioTag scenario,  ScenarioTag.MachineryInstance instance)
        {
            var scenery = new Machine();
            scenery.FriendlyName = "Machine_" + instance.MachineryDefinitionIndex;

            if (instance.MachineryDefinitionIndex == ushort.MaxValue)
            {
                Console.WriteLine($"MACH index out of range");
                return scenery;
            }

            var id = scenario.MachineryDefinitions[instance.MachineryDefinitionIndex].Machinery;
            var tag = map.GetTag(id);

            scenery.FriendlyName = tag.Name;

            var orientation = QuaternionExtensions.FromH2vOrientation(instance.Orientation);
            var xform = new TransformComponent(scenery, instance.Position, orientation);

            var components = new List<Component>();

            if (tag.Model != uint.MaxValue)
            {
                components.Add(new RenderModelComponent(scenery, new Model<BitmapTag>
                {
                    Note = $"[{tag.Id}] {tag.Name}",
                    Meshes = MeshFactory.GetRenderModel(map, tag.Model),
                    Flags = ModelFlags.Diffuse | ModelFlags.CastsShadows | ModelFlags.ReceivesShadows
                }));

                var body = PhysicsComponentFactory.CreateKinematicRigidBody(scenery, xform, map, tag.Model);
                if (body != null)
                {
                    components.Add(body);
                }
            }

            scenery.SetComponents(xform, components.ToArray());

            return scenery;
        }
    }
}
