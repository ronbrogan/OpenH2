using System;
using System.Collections.Generic;

namespace OpenH2.Core.Architecture
{
    public class Scene
    {
        public Dictionary<Guid, Entity> Entities { get; private set; } = new Dictionary<Guid, Entity>();

        public List<Component> Components { get; private set; }

        public void AddEntity(Entity e)
        {
            Entities.Add(e.Id, e);
        }
    }
}
