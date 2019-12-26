using OpenH2.Core.Architecture;
using OpenH2.Core.Factories;
using OpenH2.Core.Representations;
using OpenH2.Core.Tags;
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
        public static void AddLocators(H2vMap map, Scene destination)
        {
            var scenario = map.GetLocalTagsOfType<ScenarioTag>().First();

            foreach (var obj in scenario.Obj96s)
            {
                AddAtLocation(96, obj.Position, new Vector4(1, 0.7f, 0, 1), destination);
            }

            foreach (var obj in scenario.Obj264s)
            {
                AddAtLocation(264, obj.Position, new Vector4(0, 1, 1, 1), destination);
            }

            foreach (var obj in scenario.Obj480s)
            {
                AddAtLocation(480, obj.Position, new Vector4(1, 1, 0, 1), destination);
            }

            foreach (var obj in scenario.Obj488s)
            {
                AddAtLocation(488, obj.Position, new Vector4(0, 0, 1, 1), destination);
            }

            var bsp = map.GetLocalTagsOfType<BspTag>().First();

            foreach (var obj in bsp.InstancedGeometryInstances)
            {
                //AddAtLocation(320, obj.Position, new Vector3(1, 1, 1), destination);
            }

        }

        private static void AddAtLocation(uint id, Vector3 position, Vector4 color, Scene destination)
        {
            var item = new Scenery();
            var model = new RenderModelComponent(item)
            {
                RenderModel = ModelFactory.UnitPyramid(color)
            };

            var xform = new TransformComponent(item)
            {
                Position = position
            };

            item.SetComponents(new Component[]{
                model,
                xform
            });

            destination.Entities.Add(Guid.NewGuid(), item);

        }
    }
}
