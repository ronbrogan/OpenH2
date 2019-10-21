using System;

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
    }
}
