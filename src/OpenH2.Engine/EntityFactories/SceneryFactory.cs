﻿using OpenH2.Core.Architecture;
using OpenH2.Core.Extensions;
using OpenH2.Core.Maps.Vista;
using OpenH2.Core.Tags;
using OpenH2.Core.Tags.Scenario;
using OpenH2.Engine.Components;
using OpenH2.Engine.Entities;
using OpenH2.Engine.Factories;
using OpenH2.Foundation;
using OpenH2.Physics.Colliders;
using System.Collections.Generic;
using System.Numerics;

namespace OpenH2.Engine.EntityFactories
{
    public class SceneryFactory
    {
        public static Scenery FromInstancedGeometry(H2vMap map, BspTag bsp, BspTag.InstancedGeometryInstance instance)
        {
            var scenery = new Scenery();
            scenery.FriendlyName = "Geom";

            if (instance.Index >= bsp.InstancedGeometryDefinitions.Length)
                return scenery;

            var def = bsp.InstancedGeometryDefinitions[instance.Index];

            var renderModelMeshes = new List<Mesh<BitmapTag>>(def.Model.Meshes.Length);

            foreach (var mesh in def.Model.Meshes)
            {
                var mat = map.CreateMaterial(mesh);

                renderModelMeshes.Add(new Mesh<BitmapTag>()
                {
                    Compressed = mesh.Compressed,
                    ElementType = mesh.ElementType,
                    Indicies = mesh.Indices,
                    Note = mesh.Note,
                    RawData = mesh.RawData,
                    Verticies = mesh.Verticies,

                    Material = mat
                });
            }

            var renderModel = new RenderModelComponent(scenery, new Model<BitmapTag>
            {
                Note = $"[{bsp.Id}] {bsp.Name}//instanced//{instance.Index}",
                Meshes = renderModelMeshes.ToArray(),
                Flags = ModelFlags.Diffuse | ModelFlags.CastsShadows | ModelFlags.ReceivesShadows
            });

            var xform = new TransformComponent(scenery, instance.Position, QuaternionExtensions.From3x3Mat(instance.RotationMatrix))
            {
                Scale = new Vector3(instance.Scale),
            };

            var comps = new List<Component> { renderModel, xform };

            if (def.Vertices.Length > 0)
            {
                var geom = PhysicsComponentFactory.CreateStaticGeometry(scenery, xform, def, bsp.Shaders);
                comps.Add(geom);

                comps.Add(new RenderModelComponent(scenery, new Model<BitmapTag>
                {
                    Note = $"[{bsp.Id}] {bsp.Name}//instanced//{instance.Index}-collision",
                    Meshes = MeshFactory.GetRenderModel(geom.Collider, new Vector4(0f, 1f, 1f, 1f)),
                    Flags = ModelFlags.Wireframe | ModelFlags.IsStatic,
                    RenderLayer = RenderLayers.Collision
                }));
            }

            xform.UpdateDerivedData();

            scenery.SetComponents(comps);

            return scenery;
        }

        public static Scenery FromTag(H2vMap map, ScenarioTag scenario,  ScenarioTag.SceneryInstance instance)
        {
            var scenery = new Scenery();
            scenery.FriendlyName = "Scenery_" + instance.SceneryDefinitionIndex;

            var id = scenario.SceneryDefinitions[instance.SceneryDefinitionIndex].Scenery;
            var tag = map.GetTag(id);

            scenery.FriendlyName = tag.Name;

            var renderModel = new RenderModelComponent(scenery, new Model<BitmapTag>
            {
                Note = $"[{tag.Id}] {tag.Name}",
                Meshes = MeshFactory.GetRenderModel(map, tag.Model),
                Scale = new Vector3(1),
                Flags = ModelFlags.Diffuse | ModelFlags.CastsShadows | ModelFlags.ReceivesShadows
            });

            var orientation = QuaternionExtensions.FromH2vOrientation(instance.Orientation);
            var xform = new TransformComponent(scenery, instance.Position, orientation);

            var components = new List<Component>(2)
            {
                renderModel
            };

            var body = PhysicsComponentFactory.CreateStaticRigidBody(scenery, xform, map, tag.Model);

            if(body != null)
            {
                components.Add(body);

                components.Add(new RenderModelComponent(scenery, new Model<BitmapTag>
                {
                    Note = $"scenery//{scenery.FriendlyName}-collision",
                    Meshes = MeshFactory.GetRenderModel(body.Collider, new Vector4(0.9f, 0.5f, .24f, 1f)),
                    Flags = ModelFlags.Wireframe | ModelFlags.IsStatic,
                    RenderLayer = RenderLayers.Collision
                }));
            }

            scenery.SetComponents(xform, components.ToArray());

            return scenery;
        }
    }
}
