using OpenH2.Core.Architecture;
using OpenH2.Core.Extensions;
using OpenH2.Core.Maps;
using OpenH2.Core.Maps.Vista;
using OpenH2.Core.Tags;
using OpenH2.Core.Tags.Scenario;
using OpenH2.Engine.Components;
using OpenH2.Engine.Entities;
using OpenH2.Engine.Factories;
using OpenH2.Foundation;
using System;
using System.Collections.Generic;

namespace OpenH2.Engine.EntityFactories
{
    public class ItemFactory
    {
        public static Entity FromTag(H2vMap map, ScenarioTag scenario,  ScenarioTag.ItemCollectionPlacement instance)
        {
            if (instance.ItemCollectionReference.IsInvalid)
                return new Scenery();

            if(map.TryGetTag<BaseTag>(instance.ItemCollectionReference, out var itemTag) == false)
                throw new Exception("Unable to load itmc");
            
            if (itemTag is ItemCollectionTag itmc)
            {
                return CreateFromItemCollection(map, itmc, instance)[0];
            }

            if(itemTag is VehicleCollectionTag vehc)
            {
                return CreateFromVehicleCollection(map, vehc, instance)[0];
            }

            return new Scenery();
        }

        private static List<Entity> CreateFromItemCollection(H2vMap map, ItemCollectionTag itmc, ScenarioTag.ItemCollectionPlacement instance)
        {
            var entities = new List<Entity>();

            // I've only seen 1 item collections though
            foreach (var item in itmc.Items)
            {
                if (map.TryGetTag<BaseTag>(item.ItemTag, out var tag) == false)
                {
                    throw new Exception("No tag found for weap/equip");
                }

                TagRef<HaloModelTag> itemHlmt = default;

                if (tag is WeaponTag weap)
                    itemHlmt = weap.Hlmt;

                if (tag is EquipmentTag eqip)
                    itemHlmt = eqip.Hlmt;

                if (itemHlmt == default)
                    continue;

                var entity = new Item();
                entity.FriendlyName = tag.Name;

                var renderModel = new RenderModelComponent(entity, new Model<BitmapTag>
                {
                    Note = $"[{itmc.Id}] {itmc.Name}",
                    //Position = instance.Position,
                    //Orientation = baseRotation,
                    //Scale = new Vector3(1.3f),
                    Flags = ModelFlags.Diffuse | ModelFlags.CastsShadows | ModelFlags.ReceivesShadows,
                    Meshes = MeshFactory.GetRenderModel(map, itemHlmt)
                });

                var xform = new TransformComponent(entity, instance.Position, QuaternionExtensions.FromH2vOrientation(instance.Orientation));
                var body = PhysicsComponentFactory.CreateDynamicRigidBody(entity, xform, map, itemHlmt);

                entity.SetComponents(xform, renderModel, body);
                entities.Add(entity);
            }

            return entities;
        }

        private static List<Entity> CreateFromVehicleCollection(H2vMap map, VehicleCollectionTag vehc, ScenarioTag.ItemCollectionPlacement instance)
        {
            var entities = new List<Entity>();

            // I've only seen 1 item collections though
            foreach (var vehicle in vehc.VehicleReferences)
            {
                if (map.TryGetTag(vehicle.Vehicle, out var vehi) == false)
                {
                    throw new Exception("No tag found for vehc reference");
                }

                var entity = new Vehicle();
                entity.FriendlyName = vehi.Name;
                var xform = new TransformComponent(entity, instance.Position, QuaternionExtensions.FromH2vOrientation(instance.Orientation));
                PopulateVehicle(entity, map, xform, vehi);
                entities.Add(entity);
            }

            return entities;
        }

        public static Vehicle CreateFromVehicleInstance(H2vMap map, ScenarioTag scenario, ScenarioTag.VehicleInstance instance)
        {
            var item = new Vehicle();
            item.FriendlyName = "Vehicle_" + instance.Index;

            var def = scenario.VehicleDefinitions[instance.Index];
 
            if (map.TryGetTag(def.Vehicle, out var vehi) == false)
            {
                throw new Exception("No tag found for vehi reference");
            }

            var xform = new TransformComponent(item, instance.Position, QuaternionExtensions.FromH2vOrientation(instance.Orientation));

            PopulateVehicle(item, map, xform, vehi);

            return item;
        }

        private static void PopulateVehicle(Vehicle item, H2vMap map, TransformComponent xform, VehicleTag vehi)
        {
            item.FriendlyName = vehi.Name;
            item.SetComponents(xform,
                PhysicsComponentFactory.CreateDynamicRigidBody(item, xform, map, vehi.Hlmt),
                new RenderModelComponent(item, new Model<BitmapTag>
                {
                    Note = $"[{vehi.Id}] {vehi.Name}",
                    Flags = ModelFlags.Diffuse | ModelFlags.CastsShadows | ModelFlags.ReceivesShadows,
                    Meshes = MeshFactory.GetRenderModel(map, vehi.Hlmt)
                }));
        }
    }
}
