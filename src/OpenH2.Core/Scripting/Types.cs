namespace OpenH2.Core.Scripting
{
    using OpenH2.Core.GameObjects;
    using System.Threading.Tasks;

    public delegate Task AIScript();

    public class EntityIdentifier { }

    public class Animation { }

    public class GameObjectList 
    {
        public IGameObject[] Objects { get; set; }

        //public static implicit operator IGameObject (GameObjectList o) => o.Objects[0];
    }

    public delegate Task ScriptReference();

    public class Bsp { }
    public class Effect { }
    public class LoopingSound { }

    public class NavigationPoint { }
    public class Model { }
    public class Team { }
    public class WeaponReference { }

    public class AIBehavior { }
    public class Damage { }
    public class DamageState { }
    public class SpatialPoint { }
    public class ReferenceGet : IGameObject { }

    public class OriginScenarioAttribute : System.Attribute
    {
        public string ScenarioId { get; set; }

        public OriginScenarioAttribute(string scenarioId)
        {
            this.ScenarioId = scenarioId;
        }
    }
}