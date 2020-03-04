using System;
using System.Collections.Generic;

namespace OpenH2.Core.Architecture
{
    public abstract class Entity
    {
        public Guid Id { get; private set; }
        protected Component[] Components;

        public Entity()
        {
            this.Id = Guid.NewGuid();
        }

        public void SetComponents(Component[] components)
        {
            this.Components = components;
        }

        public bool TryGetChild<T>(out T component) where T : Component
        {
            component = null;

            foreach(var c in Components)
            {
                var t = c as T;
                if (t != null)
                {
                    component = t;
                    return true;
                }
            }

            return false;
        }

        public T[] GetChildren<T>() where T : Component
        {
            var children = new List<T>();

            foreach (var c in Components)
            {
                var t = c as T;
                if (t != null)
                {
                    children.Add(t);
                }
            }

            return children.ToArray();
        }
    }
}
