using OpenH2.Core.Architecture;
using OpenH2.Core.Extensions;
using OpenH2.Core.Factories;
using OpenH2.Core.Tags.Scenario;
using OpenH2.Engine.Components;
using OpenH2.Engine.Entities;
using OpenH2.Foundation;
using System;
using System.Diagnostics;
using System.Numerics;

namespace OpenH2.Engine.EntityFactories
{
    public class TriggerFactory
    {
        public static void WithScenarioTriggerVolume<T>(T parent, ScenarioTag tag, ScenarioTag.TriggerVolume tvDefinition, Vector3 translate = default, Quaternion rotate = default)
            where T : Entity
        {
            if(rotate == default)
                rotate = Quaternion.Identity;

            var orient = GetOrientation(tvDefinition.ForwardRotation, tvDefinition.UpRotation);

            var renderModel = ModelFactory.Cuboid(Vector3.Zero, tvDefinition.Size, new Vector4(1f, 1f, 0f, 0.5f));
            renderModel.Flags = ModelFlags.IsTransparent;
            renderModel.RenderLayer = RenderLayers.Scripting;

            var wireframeRenderModel = ModelFactory.Cuboid(Vector3.Zero, tvDefinition.Size, new Vector4(1f, 1f, 0f, 1f));
            wireframeRenderModel.Flags = ModelFlags.Wireframe;
            wireframeRenderModel.RenderLayer = RenderLayers.Scripting;

            // If the entity already has a transform, we're assuming this is relative to that
            if (parent.TryGetChild<TransformComponent>(out var parentXform))
            {
                var pos = tvDefinition.Position + translate;
                var rot = Quaternion.Multiply(rotate, orient);

                renderModel.Position = pos;
                renderModel.Orientation = rot;

                wireframeRenderModel.Position = pos;
                wireframeRenderModel.Orientation = rot;

                // transform for relative trigger geom only
                var xform = new TransformComponent(parent, pos, rot);

                parent.AppendComponents(TriggerGeometryComponent.Cuboid(parent, new CompositeTransform(parentXform, xform), tvDefinition.Size, tvDefinition.Description),
                    new RenderModelComponent(parent, renderModel),
                    new RenderModelComponent(parent, wireframeRenderModel));
            }
            else
            {
                var xform = new TransformComponent(parent, tvDefinition.Position, orient);

                parent.AppendComponents(xform,
                    TriggerGeometryComponent.Cuboid(parent, xform, tvDefinition.Size, tvDefinition.Description),
                    new RenderModelComponent(parent, renderModel),
                    new RenderModelComponent(parent, wireframeRenderModel));
            }

            Quaternion GetOrientation(Vector3 forward, Vector3 up)
            {
                var a = QuaternionExtensions.RotationFromDirection(EngineGlobals.Forward, forward);
                var b = QuaternionExtensions.RotationFromDirection(EngineGlobals.Up, up);

                return Quaternion.Concatenate(a, b);
            }
        }

        public static TriggerVolume FromScenarioTriggerVolume(ScenarioTag tag, ScenarioTag.TriggerVolume tvDefinition)
        {
            var ent = new TriggerVolume();
            ent.FriendlyName = tvDefinition.Description;

            WithScenarioTriggerVolume(ent, tag, tvDefinition);

            return ent;
        }
    }
}
