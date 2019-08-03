using OpenH2.Core.Architecture;
using OpenH2.Core.Representations;
using OpenH2.Core.Tags;
using OpenH2.Engine.Components;
using OpenH2.Engine.Entities;
using OpenH2.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace OpenH2.Engine.EntityFactories
{
    public static class PositioningEntities
    {
        public static Mesh LocatorMesh(uint mat)
        {
            return new Mesh
            {
                ElementType = MeshElementType.TriangleList,
                Indicies = new[] {
                    0, 1, 2,
                    0, 1, 3,
                    1, 2, 3,
                    2, 0, 3 },
                Verticies = new VertexFormat[] {
                    new VertexFormat(new Vector3(0, 0, 0), new Vector2(), new Vector3()),
                    new VertexFormat(new Vector3(0.5f, 0, 0), new Vector2(), new Vector3()),
                    new VertexFormat(new Vector3(0.25f, 0.5f, 0), new Vector2(), new Vector3()),
                    new VertexFormat(new Vector3(0.25f, 0.25f, 0.5f), new Vector2(), new Vector3())
                },
                MaterialIdentifier = mat
            };
        }

        public static void AddLocators(H2vMap map, Scene destination)
        {
            var scenario = map.GetLocalTagsOfType<ScenarioTag>().First();

            foreach (var obj in scenario.Obj168s)
            {
                var pos = new Vector3(obj.X, obj.Y, obj.Z);
                AddAtLocation(168, pos, new Vector3(0, 1, 0), destination);
            }

            foreach (var obj in scenario.Obj216s)
            {
                var pos = new Vector3(obj.X, obj.Y, obj.Z);
                AddAtLocation(216, pos, new Vector3(0, 0, 1), destination);
            }

            foreach (var obj in scenario.Obj232s)
            {
                var pos = new Vector3(obj.X, obj.Y, obj.Z);
                AddAtLocation(232, pos, new Vector3(1, 0, 1), destination);
            }

            foreach (var obj in scenario.Obj256s)
            {
                var pos = new Vector3(obj.X, obj.Y, obj.Z);
                AddAtLocation(256, pos, new Vector3(1, 1, 1), destination);
            }

            foreach (var obj in scenario.Obj264s)
            {
                var pos = new Vector3(obj.X, obj.Y, obj.Z);
                AddAtLocation(264, pos, new Vector3(0, 1, 1), destination);
            }

            foreach (var obj in scenario.Obj280s)
            {
                var pos = new Vector3(obj.X, obj.Y, obj.Z);
                AddAtLocation(280, pos, new Vector3(1, 1, 0), destination);
            }

            foreach (var obj in scenario.Obj808s)
            {
                var pos = new Vector3(obj.X, obj.Y, obj.Z);
                AddAtLocation(808, pos, new Vector3(0, 0, 0), destination);
            }
        }

        private static Dictionary<uint, IMaterial<BitmapTag>> Materials = new Dictionary<uint, IMaterial<BitmapTag>>();

        private static void AddAtLocation(uint id, Vector3 position, Vector3 color, Scene destination)
        {
            if(Materials.ContainsKey(id) == false)
            {
                Materials[id] = new Material<BitmapTag>() { DiffuseColor = color };
            }

            var item = new Scenery();
            var model = new RenderModelComponent(item);
            model.Position = position;
            model.Materials = Materials;
            model.Meshes = new Mesh[]
            {
                LocatorMesh(id)
            };

            item.SetComponents(new Component[]{
                model,
                new TransformComponent(item)
            });

            destination.Entities.Add(Guid.NewGuid(), item);
            
        }
    }
}
