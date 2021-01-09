using OpenH2.Core.Factories;
using OpenH2.Core.Tags.Scenario;
using OpenH2.Engine.Components;
using OpenH2.Engine.Entities;
using OpenH2.Foundation;
using System;
using System.Numerics;

namespace OpenH2.Engine.EntityFactories
{
    public class TriggerFactory
    {
        public static TriggerVolume FromScenarioTriggerVolume(ScenarioTag tag, ScenarioTag.TriggerVolume tvDefinition)
        {
            var ent = new TriggerVolume();
            ent.FriendlyName = tvDefinition.Description;

            var orient = GetOrientation(tvDefinition.Orientation, tvDefinition.OrientationAxis);

            var xform = new TransformComponent(ent, tvDefinition.Position, orient);

            var renderModel = ModelFactory.Cuboid(Vector3.Zero, tvDefinition.Size, new Vector4(1f, 1f, 0f, 0.5f));
            renderModel.Flags = ModelFlags.IsTransparent;
            renderModel.RenderLayer = RenderLayers.Scripting;

            var wireframeRenderModel = ModelFactory.Cuboid(Vector3.Zero, tvDefinition.Size, new Vector4(1f, 1f, 0f, 1f));
            wireframeRenderModel.Flags = ModelFlags.Wireframe;
            wireframeRenderModel.RenderLayer = RenderLayers.Scripting;

            ent.SetComponents(xform,
                TriggerGeometryComponent.Cuboid(ent, xform, tvDefinition.Size, tvDefinition.Description),
                new RenderModelComponent(ent, renderModel),
                new RenderModelComponent(ent, wireframeRenderModel));
            return ent;

            Quaternion GetOrientation(Vector3 angle, Vector3 axis)
            {
                //BUG? The Z value of the angle vector is not used here, and is present in ~5 triggers in the game
                var a = MathF.Atan2(angle.Y, angle.X);

                return Quaternion.CreateFromAxisAngle(axis, a);
            }
        }
    }
}
