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

            var comp = new RenderModelComponent(scenery)
            {
                RenderModel = new Model<BitmapTag>
                {
                    Note = $"[{tag.Id}] {tag.Name}",
                    Meshes = MeshFactory.GetRenderModel(map, tag.PhysicalModel),
                    Flags = ModelFlags.Diffuse | ModelFlags.CastsShadows | ModelFlags.ReceivesShadows
                }
            };

            var orientation = QuaternionExtensions.FromH2vOrientation(instance.Orientation);
            var xform = new TransformComponent(scenery, instance.Position, orientation);
            var body = PhysicsComponentFactory.CreateKinematicRigidBody(scenery, xform, map, tag.PhysicalModel);
            scenery.SetComponents(new Component[] { comp, xform, body });

            return scenery;
        }
    }
}
