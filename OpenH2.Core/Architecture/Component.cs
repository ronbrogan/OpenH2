using System;
using System.Collections.Generic;
using System.Text;

namespace OpenH2.Core.Architecture
{
    public abstract class Component
    {
        public Entity Parent { get; private set; }

        public Component(Entity parent)
        {
            this.Parent = parent;
        }

        public bool TryGetSibling<T>(out T component) where T : Component
        {
            return this.Parent.TryGetChild<T>(out component);
        }
    }
}
