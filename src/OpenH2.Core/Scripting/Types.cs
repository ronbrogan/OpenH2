namespace OpenH2.Core.Scripting
{
    using OpenH2.Core.GameObjects;
    using System.Threading.Tasks;

    public delegate Task ScriptReference();
    public delegate Task AIScript();

    public class EntityIdentifier { }

    public class GameObjectList 
    {
        public IGameObject[] Objects { get; set; }
    }

    public interface IStartingProfile { }
    public interface IAnimation { }
    public interface IBsp { }
    public interface IEffect { }
    public interface ILoopingSound : IGameObject { }
    public interface INavigationPoint { }
    public interface IModel { }
    public interface ITeam { }
    public interface IWeaponReference { }
    public interface IAIBehavior { }
    public interface IDamage { }
    public interface IDamageState { }
    public interface ISpatialPoint { }
    public interface IReferenceGet : IGameObject { }
}