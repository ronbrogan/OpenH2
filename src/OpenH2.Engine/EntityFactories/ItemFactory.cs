using OpenH2.Core.Architecture;
using OpenH2.Core.Extensions;
using OpenH2.Core.Representations;
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

            Entity item = default;
            List<Component> components = default;

            if (itemTag is ItemCollectionTag itmc)
            {
                item = new Item();
                components = CreateFromItemCollection(item, map, itmc);
            }

            if(itemTag is VehicleCollectionTag vehc)
            {
                item = new Vehicle();
                components = CreateFromVehicleCollection(item, map, vehc);
            }

            if (item == default)
                return new Scenery();

            components.Add(new TransformComponent(item)
            {
                Position = instance.Position,
                Orientation = instance.Orientation.ToQuaternion()
            });

            item.SetComponents(components.ToArray());

            return item;
        }

        private static List<Component> CreateFromItemCollection(Entity parent, H2vMap map, ItemCollectionTag itmc)
        {
            var components = new List<Component>();

            // I've only seen 1 item collections though
            foreach (var item in itmc.Items)
            {
                if (map.TryGetTag<BaseTag>(item.ItemTag, out var tag) == false)
                {
                    throw new Exception("No tag found for weap/equip");
                }

                TagRef<PhysicalModelTag> itemHlmt = default;

                if (tag is WeaponTag weap)
                    itemHlmt = weap.Hlmt;

                if (tag is EquipmentTag eqip)
                    itemHlmt = eqip.Hlmt;

                if (itemHlmt == default)
                    continue;

                components.Add(new RenderModelComponent(parent)
                {
                    RenderModel = new Model<BitmapTag>
                    {
                        Note = $"[{itmc.Id}] {itmc.Name}",
                        //Position = instance.Position,
                        //Orientation = baseRotation,
                        //Scale = new Vector3(1.3f),
                        Flags = ModelFlags.Diffuse | ModelFlags.CastsShadows | ModelFlags.ReceivesShadows,
                        Meshes = MeshFactory.GetModelForHlmt(map, itemHlmt)
                    }
                });
            }

            return components;
        }

        private static List<Component> CreateFromVehicleCollection(Entity parent, H2vMap map, VehicleCollectionTag vehc)
        {
            var components = new List<Component>();

            // I've only seen 1 item collections though
            foreach (var vehicle in vehc.VehicleReferences)
            {
                if (map.TryGetTag(vehicle.Vehicle, out var vehi) == false)
                {
                    throw new Exception("No tag found for vehc reference");
                }

                components.Add(new RenderModelComponent(parent)
                {
                    RenderModel = new Model<BitmapTag>
                    {
                        Note = $"[{vehi.Id}] {vehi.Name}",
                        //Position = instance.Position,
                        //Orientation = baseRotation,
                        //Scale = new Vector3(1.3f),
                        Flags = ModelFlags.Diffuse | ModelFlags.CastsShadows | ModelFlags.ReceivesShadows,
                        Meshes = MeshFactory.GetModelForHlmt(map, vehi.Hlmt)
                    }
                });
            }

            return components;
        }

        public static Entity CreateFromVehicleInstance(H2vMap map, ScenarioTag scenario, ScenarioTag.VehicleInstance instance)
        {
            Entity item = new Vehicle();
            List<Component> components = new List<Component>();

            var def = scenario.VehicleDefinitions[instance.Index];
 
            if (map.TryGetTag(def.Vehicle, out var vehi) == false)
            {
                throw new Exception("No tag found for vehi reference");
            }

            components.Add(new RenderModelComponent(item)
            {
                RenderModel = new Model<BitmapTag>
                {
                    Note = $"[{vehi.Id}] {vehi.Name}",
                    //Position = instance.Position,
                    //Orientation = baseRotation,
                    //Scale = new Vector3(1.3f),
                    Flags = ModelFlags.Diffuse | ModelFlags.CastsShadows | ModelFlags.ReceivesShadows,
                    Meshes = MeshFactory.GetModelForHlmt(map, vehi.Hlmt)
                }
            });

            components.Add(new TransformComponent(item)
            {
                Position = instance.Position,
                Orientation = instance.Orientation.ToQuaternion()
            });

            item.SetComponents(components.ToArray());

            return item;
        }
    }
}
