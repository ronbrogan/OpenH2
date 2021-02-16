namespace OpenH2.Core.Scripting
{
    using OpenH2.Core.Enums;
    using OpenH2.Core.GameObjects;
    using OpenH2.Core.Scripting.Execution;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Numerics;
    using System.Reflection;
    using System.Threading.Tasks;

    public delegate Task ScriptMethod();

    public class ScriptMethodReference : IScriptMethodReference
    {
        private ushort id;

        public ushort GetId() => id;

        public ScriptMethodReference(ushort id)
        {
            this.id = id;
        }

        public ScriptMethodReference(ScriptMethod method)
        {
            var attr = method.Method.GetCustomAttribute<ScriptMethodAttribute>();
            this.id = attr.Id;
        }
    }

    public interface IEntityIdentifier 
    {
        int Identifier { get; }
    }

    public struct GameObjectList : IEnumerable<IGameObject>
    {
        public static GameObjectList Empty => new GameObjectList(Array.Empty<IGameObject>());

        public GameObjectList(IGameObject?[] objects)
        {
            this.Objects = objects;
        }

        public IGameObject?[] Objects { get; set; }

        public IEnumerator<IGameObject> GetEnumerator()
        {
            return ((IEnumerable<IGameObject>)this.Objects).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.Objects.GetEnumerator();
        }

        public static bool operator ==(GameObjectList left, GameObjectList right)
        {
            return Equals(left.Objects, right.Objects);
        }

        public static bool operator !=(GameObjectList left, GameObjectList right)
        {
            return !Equals(left.Objects, right.Objects);
        }
    }

    public interface IStartingProfile : IGameObject { }
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
    public interface ISpatialPoint 
    {
        Vector3 Position { get; }
    }

    public interface ISound : IGameObject { }
    public interface IGameDifficulty 
    { 
        short Id { get; }
    }
    public interface IInternedStringId { }
    public interface IVehicleSeat { }

    public struct GameDifficulty : IGameDifficulty
    {
        public short Id { get; }

        public GameDifficulty(short id) { Id = id; }

        public static GameDifficulty Easy() => new GameDifficulty(0);
        public static GameDifficulty Normal() => new GameDifficulty(1); 
        public static GameDifficulty Heroic() => new GameDifficulty(2);
        public static GameDifficulty Legendary() => new GameDifficulty(3);

        public static implicit operator short(GameDifficulty d) => d.Id;
        public static bool operator ==(GameDifficulty f, IGameDifficulty other) => f.Id == other.Id;
        public static bool operator !=(GameDifficulty f, IGameDifficulty other) => f.Id == other.Id;
        public static bool operator ==(IGameDifficulty f, GameDifficulty other) => f.Id == other.Id;
        public static bool operator !=(IGameDifficulty f, GameDifficulty other) => f.Id == other.Id;
        public static bool operator ==(GameDifficulty f, GameDifficulty other) => f.Id == other.Id;
        public static bool operator !=(GameDifficulty f, GameDifficulty other) => f.Id == other.Id;
    }

    public interface IGameObjectDefinition<T>
    {
        T? GameObject { get; }
    }

    public interface IPlaceable
    {
        PlacementFlags PlacementFlags { get; set; }

        ushort BspIndex { get; set; }
    }

    public class EntityIdentifier : IEntityIdentifier
    {
        public int Identifier { get; private set; }

        public EntityIdentifier(int id)
        {
            this.Identifier = id;
        }
    }

    public struct ScenarioEntity<TItem> : IEntityIdentifier 
    {
        public ScenarioEntity(int id, IGameObjectDefinition<IGameObject> item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            this.Identifier = id;
            this.reference = item;
        }

        private IGameObjectDefinition<IGameObject> reference;
        public int Identifier { get; }
        public TItem? Entity => reference.GameObject == null 
            ? default(TItem)
            : (TItem)reference.GameObject;

        public static implicit operator int(ScenarioEntity<TItem> scenarioItem) => scenarioItem.Identifier;
        public static implicit operator TItem(ScenarioEntity<TItem> scenarioItem) => scenarioItem.Entity;

        public override int GetHashCode() => HashCode.Combine(
            this.Identifier.GetHashCode(), 
            this.reference.GetHashCode());
    }
}