using System;
using System.Collections.Generic;

namespace OpenH2.Core.Architecture
{
    public class Scene
    {
        public Dictionary<Guid, Entity> Entities { get; private set; } = new Dictionary<Guid, Entity>();

        public List<Component> Components { get; private set; }

        public event EntityAddEventHandler OnEntityAdd = delegate { };

        public void AddEntity(Entity e)
        {
            Entities.Add(e.Id, e);
            OnEntityAdd.Invoke(e);
        }

        public delegate void EntityAddEventHandler(Entity entity);
    }
}
