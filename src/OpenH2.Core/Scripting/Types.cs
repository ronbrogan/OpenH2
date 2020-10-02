namespace OpenH2.Core.Scripting
{
    using OpenH2.Core.GameObjects;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public delegate Task ScriptReference();
    public delegate Task AIScript();

    public class EntityIdentifier { }

    public class GameObjectList : IEnumerable<IGameObject>
    {
        public static GameObjectList Empty => new GameObjectList(Array.Empty<IGameObject>());

        public GameObjectList(IGameObject[] objects)
        {
            this.Objects = objects;
        }

        public IGameObject[] Objects { get; set; }

        public IEnumerator<IGameObject> GetEnumerator()
        {
            return ((IEnumerable<IGameObject>)this.Objects).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.Objects.GetEnumerator();
        }
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
    public interface ISound : IGameObject { }
}