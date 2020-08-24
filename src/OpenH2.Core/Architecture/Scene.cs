using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace OpenH2.Core.Architecture
{
    public class Scene
    {
        public Dictionary<Guid, Entity> Entities { get; private set; } = new Dictionary<Guid, Entity>();

        public event EntityEventHandler OnEntityAdd = delegate { };
        public event EntityEventHandler OnEntityRemove = delegate { };

        public void AddEntity(Entity e)
        {
            Entities.Add(e.Id, e);
            OnEntityAdd.Invoke(e);
        }

        public void RemoveEntity(Entity e)
        {
            Debug.Assert(Entities.ContainsKey(e.Id));

            Entities.Remove(e.Id);
            OnEntityRemove.Invoke(e);
        }

        public delegate void EntityEventHandler(Entity entity);
    }
}
