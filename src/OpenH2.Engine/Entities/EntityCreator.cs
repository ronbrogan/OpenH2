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
            var entity = SceneryFactory.FromTag(this.Map, this.Map.Scenario, scen);
            scen.GameObject = entity;
            return entity;
        }

        public Entity FromBlocInstance(ScenarioTag.BlocInstance bloc)
        {
            var entity = BlocFactory.FromTag(this.Map, this.Map.Scenario, bloc);
            bloc.GameObject = entity;
            return entity;
        }

        public Entity FromMachineryInstance(ScenarioTag.MachineryInstance mach)
        {
            var entity = MachineryFactory.FromTag(this.Map, this.Map.Scenario, mach);
            mach.GameObject = entity;
            return entity;
        }

        public Entity FromItemCollectionPlacement(ScenarioTag.ItemCollectionPlacement item)
        {
            return ItemFactory.FromTag(this.Map, this.Map.Scenario, item);
        }

        public Entity FromVehicleInstance(ScenarioTag.VehicleInstance item)
        {
            var entity = ItemFactory.CreateFromVehicleInstance(this.Map, this.Map.Scenario, item);
            item.GameObject = entity;
            return entity;
        }

        public Entity FromTriggerVolume(ScenarioTag.TriggerVolume tv)
        {
            var entity = TriggerFactory.FromScenarioTriggerVolume(this.Map.Scenario, tv);
            tv.GameObject = entity;
            return entity;
        }

        public Entity FromSquadStartingLocation(ScenarioTag.AiSquadDefinition.StartingLocation loc)
        {
            var entity = ActorFactory.FromStartingLocation(this.Map, loc);
            loc.Actor = entity;
            return entity;
        }

        public Entity FromGlobals()
        {
            var globalEntity = new GlobalSettings();
            globalEntity.FriendlyName = "Globals";
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
