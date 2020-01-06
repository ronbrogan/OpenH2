using System;

namespace OpenH2.Core.Architecture
{
    public abstract class Entity
    {
        public Guid Id { get; private set; }
        protected Component[] Components;

        private float _mass = 1f;
        public float Mass { get => IsStatic ? float.PositiveInfinity : _mass; set => _mass = value; }
        public bool IsStatic { get; set; }



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
    }
}
