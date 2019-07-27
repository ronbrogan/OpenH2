using OpenH2.Core.Architecture;
using OpenH2.Engine.Components;
using OpenH2.Engine.Stores;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenH2.Engine.Systems
{
    public class RenderCollectorSystem : WorldSystem
    {
        public RenderCollectorSystem(World world) : base(world)
        {
        }

        public override void Update(double timestep)
        {
            var renderList = this.world.GetGlobalResource<RenderListStore>();
            renderList.Clear();

            var renderables = this.world.Components<RenderModelComponent>();
            foreach(var renderable in renderables)
            {
                renderList.AddRenderModel(renderable);
            }
        }
    }
}
