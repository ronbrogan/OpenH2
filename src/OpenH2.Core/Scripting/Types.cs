namespace OpenH2.Core.Scripting
{
    using System.Threading.Tasks;

    public class AI : Unit { }
    //public class AIScript { }
    public delegate Task AIScript();
    public class Device : Entity { }
    public class EntityIdentifier { }
    public class Animation { }
    public class ObjectList 
    {
        public Entity[] Objects { get; set; }

        public static implicit operator Entity (ObjectList o) => o.Objects[0];
    }
    public class StringId { }
    //public class ScriptReference { }
    public delegate Task ScriptReference();

    public class Bsp { }
    public class Effect { }
    public class LoopingSound { }
    public class GameDifficulty { }
    public class Unit : Entity { }
    public class Scenery : Entity { }

    public class NavigationPoint { }
    public class Model { }
    public class Team { }
    public class Vehicle : Unit { }
    public class Weapon : Entity { }
    public class WeaponReference { }

    public class AIBehavior { }
    public class Damage { }
    public class DamageState { }
    public class SpatialPoint { }
    public class ReferenceGet : Entity { }

    public class OriginScenarioAttribute : System.Attribute
    {
        public string ScenarioId { get; set; }

        public OriginScenarioAttribute(string scenarioId)
        {
            this.ScenarioId = scenarioId;
        }
    }
}