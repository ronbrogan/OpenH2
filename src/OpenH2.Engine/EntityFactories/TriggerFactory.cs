using OpenH2.Core.Extensions;
using OpenH2.Core.Factories;
using OpenH2.Core.Tags.Scenario;
using OpenH2.Engine.Components;
using OpenH2.Engine.Entities;
using OpenH2.Foundation;
using System.Numerics;

namespace OpenH2.Engine.EntityFactories
{
    public class TriggerFactory
    {
        public static TriggerVolume FromScenarioTriggerVolume(ScenarioTag tag, ScenarioTag.TriggerVolume tvDefinition)
        {
            var ent = new TriggerVolume();
            ent.FriendlyName = tvDefinition.Description;

            var orient = QuaternionExtensions.FromH2vOrientation(tvDefinition.Orientation);

            var xform = new TransformComponent(ent, tvDefinition.Position, orient);

            var renderModel = ModelFactory.Cuboid(new Vector3(), tvDefinition.Size, new Vector4(1f, 1f, 0f, 0.5f));
            renderModel.Flags = ModelFlags.IsTransparent;
            renderModel.RenderLayer = RenderLayers.Scripting;

            var wireframeRenderModel = ModelFactory.Cuboid(new Vector3(), tvDefinition.Size, new Vector4(1f, 1f, 0f, 1f));
            wireframeRenderModel.Flags = ModelFlags.Wireframe;
            wireframeRenderModel.RenderLayer = RenderLayers.Scripting;

            ent.SetComponents(xform,
                TriggerGeometryComponent.Cuboid(ent, xform, tvDefinition.Size, tvDefinition.Description),
                new RenderModelComponent(ent, renderModel),
                new RenderModelComponent(ent, wireframeRenderModel));
            return ent;
        }
    }
}
