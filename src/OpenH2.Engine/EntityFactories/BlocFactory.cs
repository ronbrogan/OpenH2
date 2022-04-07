using OpenH2.Core.Architecture;
using OpenH2.Core.Enums;
using OpenH2.Core.Extensions;
using OpenH2.Core.Maps.Vista;
using OpenH2.Core.Tags;
using OpenH2.Core.Tags.Scenario;
using OpenH2.Engine.Components;
using OpenH2.Engine.Entities;
using OpenH2.Engine.Factories;
using OpenH2.Foundation;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace OpenH2.Engine.EntityFactories
{
    public class BlocFactory
    {
        private static Vector4 PackColorChange(ScenarioTag.BlocInstance instance)
        {
            if (instance.ActiveColorChanges == 0) return Vector4.Zero;

            var primary = (instance.PrimaryColorBgr[2] << 24)
                | (instance.PrimaryColorBgr[1] << 16)
                | (instance.PrimaryColorBgr[0] << 8)
                | (instance.ActiveColorChanges.HasFlag(ColorChangeFlags.Primary) ? 1 : 0);

            var secondary = (instance.SecondaryColorBgr[2] << 24)
                | (instance.SecondaryColorBgr[1] << 16)
                | (instance.SecondaryColorBgr[0] << 8)
                | (instance.ActiveColorChanges.HasFlag(ColorChangeFlags.Secondary) ? 1 : 0);

            var tertiary = (instance.TertiaryColorBgr[2] << 24)
                | (instance.TertiaryColorBgr[1] << 16)
                | (instance.TertiaryColorBgr[0] << 8)
                | (instance.ActiveColorChanges.HasFlag(ColorChangeFlags.Tertiary) ? 1 : 0);

            var quaternary = (instance.QuaternaryColorBgr[2] << 24)
                | (instance.QuaternaryColorBgr[1] << 16)
                | (instance.QuaternaryColorBgr[0] << 8)
                | (instance.ActiveColorChanges.HasFlag(ColorChangeFlags.Quaternary) ? 1 : 0);

            return new Vector4(BitConverter.Int32BitsToSingle(primary),
                BitConverter.Int32BitsToSingle(secondary),
                BitConverter.Int32BitsToSingle(tertiary),
                BitConverter.Int32BitsToSingle(quaternary));
        }

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
                Meshes = MeshFactory.GetRenderModel(map, tag.PhysicalModel),
                ColorChangeData = PackColorChange(instance)
            }));

            var orientation = QuaternionExtensions.FromH2vOrientation(instance.Orientation);
            var xform = new TransformComponent(scenery, instance.Position, orientation);

            var body = PhysicsComponentFactory.CreateDynamicRigidBody(scenery, xform, map, tag.PhysicalModel);

            if(body != null)
            {
                components.Add(body);
                components.Add(new RenderModelComponent(scenery, new Model<BitmapTag>
                {
                    Note = $"bloc//{scenery.FriendlyName}-collision",
                    Meshes = MeshFactory.GetRenderModel(body.Collider, new Vector4(0.19f, 0.47f, 0.15f, 1f)),
                    Flags = ModelFlags.Wireframe | ModelFlags.IsStatic,
                    RenderLayer = RenderLayers.Collision
                }));
            }

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
