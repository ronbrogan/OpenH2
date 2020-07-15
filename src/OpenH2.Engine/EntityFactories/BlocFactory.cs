using OpenH2.Core.Architecture;
using OpenH2.Core.Extensions;
using OpenH2.Core.Representations;
using OpenH2.Core.Tags;
using OpenH2.Core.Tags.Scenario;
using OpenH2.Engine.Components;
using OpenH2.Engine.Entities;
using OpenH2.Engine.Factories;
using OpenH2.Foundation;
using System.Numerics;

namespace OpenH2.Engine.EntityFactories
{
    public class BlocFactory
    {
        public static Bloc FromTag(H2vMap map, ScenarioTag scenario,  ScenarioTag.BlocInstance instance)
        {
            var scenery = new Bloc();

            var bloc = scenario.BlocDefinitions[instance.BlocDefinitionIndex].Bloc;
            map.TryGetTag(bloc, out var tag);

            var comp = new RenderModelComponent(scenery)
            {
                RenderModel = new Model<BitmapTag>
                {
                    Note = $"[{tag.Id}] {tag.Name}",
                    //Position = instance.Position,
                    //Orientation = instance.Orientation.ToQuaternion(),
                    Flags = ModelFlags.Diffuse | ModelFlags.CastsShadows | ModelFlags.ReceivesShadows,
                    Meshes = MeshFactory.GetRenderModel(map, tag.PhysicalModel)
                }
            };

            var orientation = QuaternionExtensions.FromH2vOrientation(instance.Orientation);
            var xform = new TransformComponent(scenery, instance.Position, orientation);

            var body = PhysicsComponentFactory.CreateDynamicRigidBody(scenery, xform, map, tag.PhysicalModel);

            var comOffset = Vector3.Zero;

            if (map.TryGetTag(tag.PhysicalModel, out var hlmt) &&
                map.TryGetTag(hlmt.PhysicsModel, out var phmo) &&
                phmo.BodyParameters.Length > 0)
            {
                comOffset = phmo.BodyParameters[0].CenterOfMass;
            }

            var centerOfMass = new BoundsComponent(scenery, comOffset - new Vector3(0.02f), comOffset + new Vector3(0.02f), new Vector4(1f, 1f, 0, 1f));
            var origin = new BoundsComponent(scenery, new Vector3(-0.02f), new Vector3(0.02f), new Vector4(0, 1f, 0, 1f));

            var originalTag = new OriginalTagComponent(scenery, instance);

            scenery.SetComponents(new Component[] { comp, centerOfMass, origin, xform, body, originalTag });

            return scenery;
        }
    }
}
