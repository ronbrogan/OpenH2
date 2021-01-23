using OpenH2.Core.Architecture;
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
    public class BlocFactory
    {
        public static Bloc FromTag(H2vMap map, ScenarioTag scenario,  ScenarioTag.BlocInstance instance)
        {
            var scenery = new Bloc();
            var components = new List<Component>();

            var bloc = scenario.BlocDefinitions[instance.BlocDefinitionIndex].Bloc;
            var tag = map.GetTag(bloc);

            scenery.FriendlyName = tag.Name;

            components.Add(new RenderModelComponent(scenery, new Model<BitmapTag>
            {
                Note = $"[{tag.Id}] {tag.Name}",
                Flags = ModelFlags.Diffuse | ModelFlags.CastsShadows | ModelFlags.ReceivesShadows,
                Meshes = MeshFactory.GetRenderModel(map, tag.PhysicalModel)
            }));

            var orientation = QuaternionExtensions.FromH2vOrientation(instance.Orientation);
            var xform = new TransformComponent(scenery, instance.Position, orientation);

            var body = PhysicsComponentFactory.CreateDynamicRigidBody(scenery, xform, map, tag.PhysicalModel);


            if (body.Collider is TriangleMeshCollider triMeshCollider)
            {
                components.Add(new RenderModelComponent(scenery, new Model<BitmapTag>
                {
                    Note = $"bloc//{scenery.FriendlyName}-collision",
                    Meshes = MeshFactory.GetRenderModel(triMeshCollider, new Vector4(0.19f, 0.47f, 0.15f, 1f)),
                    Flags = ModelFlags.Wireframe | ModelFlags.IsStatic,
                    RenderLayer = RenderLayers.Collision
                }));
            }
            else if (body.Collider is TriangleModelCollider triCollider)
            {
                components.Add(new RenderModelComponent(scenery, new Model<BitmapTag>
                {
                    Note = $"bloc//{scenery.FriendlyName}-collision",
                    Meshes = MeshFactory.GetRenderModel(triCollider, new Vector4(0.19f, 0.47f, 0.15f, 1f)),
                    Flags = ModelFlags.Wireframe | ModelFlags.IsStatic,
                    RenderLayer = RenderLayers.Collision
                }));
            }
            else if (body.Collider is ConvexModelCollider convexCollider)
            {
                components.Add(new RenderModelComponent(scenery, new Model<BitmapTag>
                {
                    Note = $"bloc//{scenery.FriendlyName}-collision",
                    Meshes = MeshFactory.GetRenderModel(convexCollider, new Vector4(0.19f, 0.47f, 0.15f, 1f)),
                    Flags = ModelFlags.Wireframe | ModelFlags.IsStatic,
                    RenderLayer = RenderLayers.Collision
                }));
            }
            else if (body.Collider is ConvexMeshCollider convexMeshCollider)
            {
                components.Add(new RenderModelComponent(scenery, new Model<BitmapTag>
                {
                    Note = $"bloc//{scenery.FriendlyName}-collision",
                    Meshes = MeshFactory.GetRenderModel(convexMeshCollider, new Vector4(0.19f, 0.47f, 0.15f, 1f)),
                    Flags = ModelFlags.Wireframe | ModelFlags.IsStatic,
                    RenderLayer = RenderLayers.Collision
                }));
            }

            components.Add(body);

            var comOffset = Vector3.Zero;

            if (map.TryGetTag(tag.PhysicalModel, out var hlmt) &&
                map.TryGetTag(hlmt.PhysicsModel, out var phmo) &&
                phmo.BodyParameters.Length > 0)
            {
                comOffset = phmo.BodyParameters[0].CenterOfMass;
            }

            components.Add(new BoundsComponent(scenery, comOffset - new Vector3(0.02f), comOffset + new Vector3(0.02f), new Vector4(1f, 1f, 0, 1f)));
            components.Add(new BoundsComponent(scenery, new Vector3(-0.02f), new Vector3(0.02f), new Vector4(0, 1f, 0, 1f)));

            components.Add(new OriginalTagComponent(scenery, instance));

            scenery.SetComponents(xform, components.ToArray());

            return scenery;
        }
    }
}
