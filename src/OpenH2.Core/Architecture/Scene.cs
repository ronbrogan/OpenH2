using OpenH2.Core.Representations;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace OpenH2.Core.Architecture
{
    public class Scene
    {
        private readonly IEntityCreator entityCreator;
        public event EntityEventHandler OnEntityAdd = delegate { };
        public event EntityEventHandler OnEntityRemove = delegate { };

        public H2vMap Map { get; }
        public Dictionary<Guid, Entity> Entities { get; private set; } = new Dictionary<Guid, Entity>();
        public Dictionary<object, Entity> ScenarioSourcedEntities { get; } = new Dictionary<object, Entity>();

        public Scene(H2vMap map, IEntityCreator entityCreator)
        {
            this.Map = map;
            this.entityCreator = entityCreator;
        }

        public void AddEntity(Entity e)
        {
            Entities.Add(e.Id, e);
            OnEntityAdd.Invoke(e);
        }

        public void AddScenarioEntity(object entityDef, Entity e)
        {
            this.ScenarioSourcedEntities.Add(entityDef, e);
            this.AddEntity(e);
        }

        public void RemoveEntity(Entity e)
        {
            Debug.Assert(Entities.ContainsKey(e.Id));

            Entities.Remove(e.Id);
            OnEntityRemove.Invoke(e);
        }

        public delegate void EntityEventHandler(Entity entity);

        public void Load()
        {
            var terrains = this.Map.Scenario.Terrains;

            foreach (var terrain in terrains)
            {
                this.Map.TryGetTag(terrain.Bsp, out var bsp);

                this.AddScenarioEntity(bsp, entityCreator.FromBsp(bsp));

                foreach (var instance in bsp.InstancedGeometryInstances)
                {
                    this.AddScenarioEntity(instance, entityCreator.FromInstancedGeometry(bsp, instance));
                }
            }

            foreach (var sky in this.Map.Scenario.SkyboxInstances)
            {
                if (sky.Skybox == uint.MaxValue)
                    continue;

                this.AddScenarioEntity(sky, entityCreator.FromSkyboxInstance(sky));
            }

            foreach (var scen in this.Map.Scenario.SceneryInstances)
            {
                if (scen.SceneryDefinitionIndex == ushort.MaxValue)
                    continue;

                this.AddScenarioEntity(scen, entityCreator.FromSceneryInstance(scen));
            }

            foreach (var bloc in this.Map.Scenario.BlocInstances)
            {
                this.AddScenarioEntity(bloc, entityCreator.FromBlocInstance(bloc));
            }

            foreach (var mach in this.Map.Scenario.MachineryInstances)
            {
                this.AddScenarioEntity(mach, entityCreator.FromMachineryInstance(mach));
            }

            foreach (var item in this.Map.Scenario.ItemCollectionPlacements)
            {
                this.AddScenarioEntity(item, entityCreator.FromItemCollectionPlacement(item));
            }

            foreach (var item in this.Map.Scenario.VehicleInstances)
            {
                // HACK: sometimes maxval, headlong
                if (item.Index == ushort.MaxValue)
                    continue;

                this.AddScenarioEntity(item, entityCreator.FromVehicleInstance(item));
            }

            foreach (var tv in this.Map.Scenario.TriggerVolumes)
            {
                this.AddScenarioEntity(tv, entityCreator.FromTriggerVolume(tv));
            }

            this.AddEntity(entityCreator.FromGlobals());
        }
    }
}
