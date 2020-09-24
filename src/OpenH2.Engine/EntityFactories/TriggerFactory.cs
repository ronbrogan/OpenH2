using Microsoft.CodeAnalysis;
using OpenH2.Core.Architecture;
using OpenH2.Core.Extensions;
using OpenH2.Core.Factories;
using OpenH2.Core.Tags.Scenario;
using OpenH2.Engine.Components;
using OpenH2.Engine.Entities;
using System.Collections.Generic;
using System.Numerics;

namespace OpenH2.Engine.EntityFactories
{
    public class TriggerFactory
    {
        public static TriggerVolume FromScenarioTriggerVolume(ScenarioTag tag, ScenarioTag.TriggerVolume tvDefinition)
        {
            var ent = new TriggerVolume();

            var orient = QuaternionExtensions.FromH2vOrientation(tvDefinition.Orientation);

            var xform = new TransformComponent(ent, tvDefinition.Position, orient);

            var renderModel = ModelFactory.Cuboid(new Vector3(), tvDefinition.Size, new Vector4(1f, 1f, 0f, 0.5f));
            renderModel.Flags = Foundation.ModelFlags.Wireframe | 
                Foundation.ModelFlags.IsTransparent | 
                Foundation.ModelFlags.DoubleSided;
            
            ent.SetComponents(xform,
                TriggerGeometryComponent.Cuboid(ent, xform, tvDefinition.Size, tvDefinition.Description),
                new RenderModelComponent(ent, renderModel));
            return ent;
        }
    }
}
