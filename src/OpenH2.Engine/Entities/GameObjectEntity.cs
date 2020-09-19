using OpenH2.Core.Architecture;
using OpenH2.Core.GameObjects;
using OpenH2.Engine.Components;
using OpenH2.Physics.Proxying;
using System;
using System.Numerics;

namespace OpenH2.Engine.Entities
{
    public abstract class GameObjectEntity : Entity, IGameObject
    {
        public TransformComponent Transform { get; protected set; }
        public IPhysicsProxy Physics { get; protected set; }

        public GameObjectEntity()
        {
            this.Components = Array.Empty<Component>();
        }

        public virtual void Hide()
        {
            throw new NotImplementedException();
        }

        public virtual void SetShield(float vitality)
        {
            throw new NotImplementedException();
        }

        public virtual void Show()
        {
            throw new NotImplementedException();
        }

        public virtual void TeleportTo(Vector3 position)
        {
            throw new NotImplementedException();
        }
    }
}
