using OpenH2.Core.GameObjects;
using OpenH2.Core.Tags;
using OpenH2.Core.Tags.Scenario;

namespace OpenH2.Core.Architecture
{
    public interface IEntityCreator
    {
        Entity FromBsp(BspTag bsp);
        Entity FromInstancedGeometry(BspTag bsp, BspTag.InstancedGeometryInstance instance);
        Entity FromSkyboxInstance(ScenarioTag.SkyboxInstance sky);
        Entity FromSceneryInstance(ScenarioTag.SceneryInstance scen);
        Entity FromBlocInstance(ScenarioTag.BlocInstance bloc);
        Entity FromMachineryInstance(ScenarioTag.MachineryInstance mach);
        Entity FromItemCollectionPlacement(ScenarioTag.ItemCollectionPlacement item);
        Entity FromVehicleInstance(ScenarioTag.VehicleInstance item);
        Entity FromTriggerVolume(ScenarioTag.TriggerVolume tv);
        Entity FromSquadStartingLocation(ScenarioTag.AiSquadDefinition.StartingLocation loc);
        Entity FromGlobals();
    }
}
