using OpenH2.Core.Architecture;
using OpenH2.Core.Extensions;
using OpenH2.Core.Representations;
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
        public static Entity FromTag(H2vMap map, ScenarioTag scenario,  ScenarioTag.MachineryInstance instance)
        {
            var scenery = new Machine();

            if(instance.MachineryDefinitionIndex == ushort.MaxValue)
            {
                Console.WriteLine($"MACH index out of range");
                return scenery;
            }

            var id = scenario.MachineryDefinitions[instance.MachineryDefinitionIndex].Machinery;
            var tag = map.GetTag(id);

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
