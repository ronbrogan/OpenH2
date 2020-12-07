using OpenH2.Core.Enums;
using OpenH2.Core.Maps;
using OpenH2.Core.Scripting;
using OpenH2.Core.Tags.Scenario;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace OpenH2.Core.Architecture
{
    public class Scene
    {
        public IEntityCreator EntityCreator { get; }
        public event EntityEventHandler OnEntityAdd = delegate { };
        public event EntityEventHandler OnEntityRemove = delegate { };

        public H2vMap Map { get; }
        public ScenarioTag Scenario => Map.Scenario;

        public Dictionary<Guid, Entity> Entities { get; private set; } = new Dictionary<Guid, Entity>();

        public Scene(H2vMap map, IEntityCreator entityCreator)
        {
            this.Map = map;
            this.EntityCreator = entityCreator;
        }

        public void AddEntity(Entity e)
        {
            Entities.Add(e.Id, e);
            OnEntityAdd.Invoke(e);
        }

        public void RemoveEntity(Entity e)
        {
            Entities.Remove(e.Id);
            OnEntityRemove.Invoke(e);
        }

        public delegate void EntityEventHandler(Entity entity);

        public void Load()
        {
            Func<IPlaceable, bool> shouldPlace = (IPlaceable p) => p.PlacementFlags.HasFlag(PlacementFlags.NotAutomatically) == false;
            var terrains = this.Scenario.Terrains;

            foreach (var terrain in terrains)
            {
                this.Map.TryGetTag(terrain.Bsp, out var bsp);

                this.AddEntity(EntityCreator.FromBsp(bsp));

                foreach (var instance in bsp.InstancedGeometryInstances)
                {
                    this.AddEntity(EntityCreator.FromInstancedGeometry(bsp, instance));
                }
            }

            foreach (var sky in this.Map.Scenario.SkyboxInstances)
            {
                if (sky.Skybox == uint.MaxValue)
                    continue;

                this.AddEntity(EntityCreator.FromSkyboxInstance(sky));
            }

            foreach (ScenarioTag.SceneryInstance scen in this.Map.Scenario.SceneryInstances.Where(shouldPlace))
            {
                if (scen.SceneryDefinitionIndex == ushort.MaxValue)
                    continue;

                this.AddEntity(EntityCreator.FromSceneryInstance(scen));
            }

            foreach (ScenarioTag.BlocInstance bloc in this.Map.Scenario.BlocInstances.Where(shouldPlace))
            {
                this.AddEntity(EntityCreator.FromBlocInstance(bloc));
            }

            foreach (ScenarioTag.MachineryInstance mach in this.Map.Scenario.MachineryInstances.Where(shouldPlace))
            {
                this.AddEntity(EntityCreator.FromMachineryInstance(mach));
            }

            foreach (var item in this.Map.Scenario.ItemCollectionPlacements)
            {
                this.AddEntity(EntityCreator.FromItemCollectionPlacement(item));
            }

            foreach (var item in this.Map.Scenario.VehicleInstances)
            {
                // HACK: sometimes maxval, headlong
                if (item.Index == ushort.MaxValue)
                    continue;

                this.AddEntity(EntityCreator.FromVehicleInstance(item));
            }

            foreach (var tv in this.Map.Scenario.TriggerVolumes)
            {
                this.AddEntity(EntityCreator.FromTriggerVolume(tv));
            }

            this.AddEntity(EntityCreator.FromGlobals());
        }

        public void CreateEntity(ScenarioTag.EntityReference def)
        {
            Entity? newEntity = def.ItemType switch
            {
                ScenarioTag.WellKnownVarType.Biped => null,
                ScenarioTag.WellKnownVarType.Vehicle => EntityCreator.FromVehicleInstance(Scenario.VehicleInstances[def.Index]),
                ScenarioTag.WellKnownVarType.Weapon => null,
                ScenarioTag.WellKnownVarType.Equipment => null,
                ScenarioTag.WellKnownVarType.Scenery => EntityCreator.FromSceneryInstance(Scenario.SceneryInstances[def.Index]),
                ScenarioTag.WellKnownVarType.Machinery => EntityCreator.FromMachineryInstance(Scenario.MachineryInstances[def.Index]),
                ScenarioTag.WellKnownVarType.Controller => null,
                ScenarioTag.WellKnownVarType.Sound => null,
                ScenarioTag.WellKnownVarType.Bloc => EntityCreator.FromBlocInstance(Scenario.BlocInstances[def.Index]),
                ScenarioTag.WellKnownVarType.Undef => null,
                _ => throw new NotImplementedException(),
            };

            if(newEntity != null)
            {
                this.AddEntity(newEntity);
            }
        }
    }
}
