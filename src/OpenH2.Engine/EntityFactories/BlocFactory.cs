using OpenH2.Core.Architecture;
using OpenH2.Core.Representations;
using OpenH2.Core.Tags;
using OpenH2.Core.Tags.Scenario;
using OpenH2.Engine.Components;
using OpenH2.Engine.Entities;
using OpenH2.Engine.Factories;
using OpenH2.Foundation;
using OpenH2.Physics.Colliders;
using System.Numerics;

namespace OpenH2.Engine.EntityFactories
{
    public class BlocFactory
    {
        public static Scenery FromTag(H2vMap map, ScenarioTag scenario,  ScenarioTag.BlocInstance instance)
        {
            var scenery = new Scenery();

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
                    Meshes = MeshFactory.GetModelForHlmt(map, tag.PhysicalModel, out var least, out var most)
                }
            };

            var orientation = Quaternion.CreateFromYawPitchRoll(instance.Orientation.Y, instance.Orientation.Z, instance.Orientation.X);
            var xform = new TransformComponent(scenery, instance.Position, orientation);

            var inertiaTensor = Matrix4x4.Identity;
            var comOffset = Vector3.Zero;

            if(map.TryGetTag(tag.PhysicalModel, out var hlmt) &&
                map.TryGetTag(hlmt.PhysicsModel, out var phmo) &&
                phmo.BodyParameters.Length > 0)
            {
                inertiaTensor = phmo.BodyParameters[0].InertiaTensor;
                comOffset = phmo.BodyParameters[0].CenterOfMass;
            }

            var body = new RigidBodyComponent(scenery, xform, inertiaTensor, comOffset);
            body.Collider = new BoxCollider(xform, (most-least)/2, comOffset);

            var modelBounds = new BoundsComponent(scenery, least, most);
            var centerOfMass = new BoundsComponent(scenery, comOffset - new Vector3(0.02f), comOffset + new Vector3(0.02f), new Vector4(1f, 1f, 0, 1f));
            var origin = new BoundsComponent(scenery, new Vector3(-0.02f), new Vector3(0.02f), new Vector4(0, 1f, 0, 1f));

            scenery.SetComponents(new Component[] { comp, modelBounds, centerOfMass, origin, xform, body });

            return scenery;
        }
    }
}
