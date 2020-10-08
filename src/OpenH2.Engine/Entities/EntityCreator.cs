using OpenH2.Core.Architecture;
using OpenH2.Core.Maps;
using OpenH2.Core.Tags;
using OpenH2.Core.Tags.Scenario;
using OpenH2.Engine.Components.Globals;
using OpenH2.Engine.EntityFactories;
using OpenH2.Physics.Core;

namespace OpenH2.Engine.Entities
{
    public class EntityCreator : IEntityCreator
    {
        public H2vMap Map { get; }

        public EntityCreator(H2vMap map)
        {
            this.Map = map;
        }

        public Entity FromBsp(BspTag bsp)
        {
            return TerrainFactory.FromBspData(this.Map, bsp);
        }

        public Entity FromInstancedGeometry(BspTag bsp, BspTag.InstancedGeometryInstance instance)
        {
            return SceneryFactory.FromInstancedGeometry(this.Map, bsp, instance);
        }

        public Entity FromSkyboxInstance(ScenarioTag.SkyboxInstance sky)
        {
            return SkyboxFactory.FromTag(this.Map, this.Map.Scenario, sky);
        }

        public Entity FromSceneryInstance(ScenarioTag.SceneryInstance scen)
        {
            return SceneryFactory.FromTag(this.Map, this.Map.Scenario, scen);
        }

        public Entity FromBlocInstance(ScenarioTag.BlocInstance bloc)
        {
            return BlocFactory.FromTag(this.Map, this.Map.Scenario, bloc);
        }

        public Entity FromMachineryInstance(ScenarioTag.MachineryInstance mach)
        {
            return MachineryFactory.FromTag(this.Map, this.Map.Scenario, mach);
        }

        public Entity FromItemCollectionPlacement(ScenarioTag.ItemCollectionPlacement item)
        {
            return ItemFactory.FromTag(this.Map, this.Map.Scenario, item);
        }

        public Entity FromVehicleInstance(ScenarioTag.VehicleInstance item)
        {
            return ItemFactory.CreateFromVehicleInstance(this.Map, this.Map.Scenario, item);
        }

        public Entity FromTriggerVolume(ScenarioTag.TriggerVolume tv)
        {
            return TriggerFactory.FromScenarioTriggerVolume(this.Map.Scenario, tv);
        }

        public Entity FromGlobals()
        {
            var globalEntity = new GlobalSettings();
            var globalMaterials = new MaterialListComponent(globalEntity);

            for (var i = 0; i < this.Map.Globals.MaterialDefinitions.Length; i++)
            {
                var def = this.Map.Globals.MaterialDefinitions[i];
                var mat = new PhysicsMaterial(i, def.Friction, def.Friction, def.Restitution);

                globalMaterials.AddPhysicsMaterial(mat);
            }

            globalEntity.SetComponents(new Component[] { globalMaterials });
            return globalEntity;
        }
    }
}
