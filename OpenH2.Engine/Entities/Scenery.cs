using OpenH2.Core.Architecture;
using OpenH2.Engine.Components;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenH2.Engine.Entities
{
    public class Scenery : Entity
    {
        public Scenery()
        {
            var renderComponent = new RenderModelComponent(this);

            this.Components = new Component[]
            {
                renderComponent,
                new TransformComponent(this)
            };
        }

        public void SetComponents(Component[] components)
        {
            this.Components = components;
        }
    }
}
