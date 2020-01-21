using OpenH2.Core.Architecture;
using OpenH2.Core.Factories;
using OpenH2.Core.Representations;
using OpenH2.Core.Tags;
using OpenH2.Core.Tags.Scenario;
using OpenH2.Engine.Components;
using OpenH2.Engine.Entities;
using OpenH2.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace OpenH2.Engine.EntityFactories
{
    public static class PositioningEntities
    {
        private static Vector4[] colors = new Vector4[]
        {
            new Vector4(0.9019608f, 0.09803922f, 0.29411766f, 1),
            new Vector4(0.23529412f, 0.7058824f, 0.29411766f, 1),
            new Vector4(1f, 0.88235295f, 0.09803922f, 1),
            new Vector4(0f, 0.50980395f, 0.78431374f, 1),
            new Vector4(0.9607843f, 0.50980395f, 0.1882353f, 1),
            new Vector4(0.5686275f, 0.11764706f, 0.7058824f, 1),
            new Vector4(0.27450982f, 0.9411765f, 0.9411765f, 1),
            new Vector4(0.9411765f, 0.19607843f, 0.9019608f, 1),
            new Vector4(0.8235294f, 0.9607843f, 0.23529412f, 1),
            new Vector4(0.98039216f, 0.74509805f, 0.74509805f, 1),
            new Vector4(0f, 0.5019608f, 0.5019608f, 1),
            new Vector4(0.9019608f, 0.74509805f, 1f, 1),
            new Vector4(0.6666667f, 0.43137255f, 0.15686275f, 1),
            new Vector4(1f, 0.98039216f, 0.78431374f, 1),
            new Vector4(0.5019608f, 0f, 0f, 1),
            new Vector4(0.6666667f, 1f, 0.7647059f, 1),
            new Vector4(0.5019608f, 0.5019608f, 0f, 1),
            new Vector4(1f, 0.84313726f, 0.7058824f, 1),
            new Vector4(0f, 0f, 0.5019608f, 1),
            new Vector4(0.5019608f, 0.5019608f, 0.5019608f, 1),
            new Vector4(1f, 1f, 1f, 1),
            new Vector4(0f, 0f, 0f, 1)
        };

        public static void AddLocators(H2vMap map, Scene destination)
        {
            var scenario = map.GetLocalTagsOfType<ScenarioTag>().First();

            foreach (var obj in scenario.Obj96s)
            {
                // Corpses, for sure...
                AddAtLocation(96, obj.Position, colors[0], destination);
            }

            foreach (var obj in scenario.VehicleInstances)
            {
                // Turrets?
                AddAtLocation(112, obj.Position, colors[1], destination);
            }

            foreach (var obj in scenario.Obj264s)
            {
                // Script triggers?
                AddAtLocation(264, obj.Position, colors[3], destination);
            }

            foreach (var obj in scenario.Obj480s)
            {
                // Zone triggers?
                AddAtLocation(480, obj.Position, colors[4], destination);
            }

            foreach (var obj in scenario.Obj488s)
            {
                AddAtLocation(488, obj.Position, colors[5], destination);
            }
        }

        private static void AddAtLocation(uint id, Vector3 position, Vector4 color, Scene destination)
        {
            var item = new Scenery();
            var model = new RenderModelComponent(item)
            {
                RenderModel = ModelFactory.UnitPyramid(color)
            };

            var xform = new TransformComponent(item, position);

            item.SetComponents(new Component[]{
                model,
                xform
            });

            destination.Entities.Add(Guid.NewGuid(), item);

        }
    }
}
