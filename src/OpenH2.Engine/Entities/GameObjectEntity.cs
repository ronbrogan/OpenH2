using OpenH2.Core.Architecture;
using OpenH2.Core.GameObjects;
using OpenH2.Engine.Components;
using OpenH2.Physics.Proxying;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace OpenH2.Engine.Entities
{
    public abstract class GameObjectEntity : Entity, IGameObject
    {
        public TransformComponent Transform { get; protected set; }
        public IPhysicsProxy Physics { get; protected set; }

        public Vector3 Position => Transform.Position;
        public Vector3 EyeOffset { get; internal set; } = Vector3.Zero;
        public Quaternion Orientation => Transform.Orientation;

        public float Shield { get; set; }
        public float Health { get; set; }

        public IGameObject Parent { get; }

        public IAiActorDefinition Ai { get; }

        public bool IsAlive { get; }

        public GameObjectEntity()
        {
            this.Components = Array.Empty<Component>();
        }

        public void SetComponents(TransformComponent xform,
            params Component[] rest)
        {
            this.Transform = xform;

            var allComponents = new List<Component>();
            allComponents.Add(xform);
            allComponents.AddRange(rest);
            this.SetComponents(allComponents);
        }

        public virtual void Hide()
        {
        }

        public virtual void Show()
        {
        }

        public virtual void TeleportTo(Vector3 position)
        {
            this.Physics.Move(position, 16);
        }

        public void Scale(float scale)
        {
        }

        public void Attach(IGameObject entity)
        {
        }

        public void Detach(IGameObject child)
        {
        }
    }
}
