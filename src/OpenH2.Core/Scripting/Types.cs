namespace OpenH2.Core.Scripting
{
    using System.Threading.Tasks;

    public class AI : Unit { }
    //public class AIScript { }
    public delegate Task AIScript();
    public class Device : Entity { }
    public class EntityIdentifier { }
    public class Animation { }
    public class EntityList 
    {
        public Entity[] Objects { get; set; }

        public static implicit operator Entity (EntityList o) => o.Objects[0];
    }

    public delegate Task ScriptReference();

    public class Bsp { }
    public class Effect { }
    public class LoopingSound { }
    public class Unit : Entity { }

    public class NavigationPoint { }
    public class Model { }
    public class Team { }
    public class Vehicle : Unit { }
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