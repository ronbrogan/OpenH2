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

            var modelBounds = new BoundsComponent(scenery, least, most);

            var xform = new TransformComponent(scenery)
            {
                Position = instance.Position,
                Orientation = Quaternion.CreateFromYawPitchRoll(instance.Orientation.Y, instance.Orientation.Z, instance.Orientation.X)
            };

            scenery.SetComponents(new Component[] { comp, modelBounds, xform });

            return scenery;
        }
    }
}
