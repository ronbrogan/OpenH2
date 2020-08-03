namespace OpenH2.Core.Scripting
{
    public class AI { }
    //public class AIScript { }
    public delegate void AIScript();
    public class Device { }
    public class EntityIdentifier { }
    public class Entity { }
    public class Trigger { }
    public class Animation { }
    public class LocationFlag { }
    public class ObjectList { }
    public class StringId { }
    //public class ScriptReference { }
    public delegate void ScriptReference();
    public class DeviceGroup { }
    public class AIOrders { }
    public class Bsp { }
    public class Effect { }
    public class LoopingSound { }
    public class GameDifficulty { }
    public class Unit : Entity { }
    public class Scenery { }
    public class VehicleSeat { }
    public class Equipment { }
    public class NavigationPoint { }
    public class Model { }
    public class Team { }
    public class Vehicle { }
    public class Weapon { }
    public class CameraPathTarget { }
    public class CinematicTitle { }
    public class AIBehavior { }
    public class Damage { }
    public class DamageState { }
    public class SpatialPoint { }
    public class ReferenceGet { }

    public class OriginScenarioAttribute : System.Attribute
    {
        public string ScenarioId { get; set; }

        public OriginScenarioAttribute(string scenarioId)
        {
            this.ScenarioId = scenarioId;
        }
    }
}