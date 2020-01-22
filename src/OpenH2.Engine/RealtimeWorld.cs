using OpenH2.Core.Architecture;
using OpenH2.Engine.Stores;
using OpenH2.Engine.Systems;
using OpenH2.Rendering.Abstractions;
using System.Collections.Generic;

namespace OpenH2.Engine
{
    public class RealtimeWorld : World
    {
        private List<object> globalResources = new List<object>();

        public RealtimeWorld(Engine engine)
        {
            // new up systems
            Systems.Add(new OpenTKInputSystem(this));
            Systems.Add(new MoverSystem(this));

            Systems.Add(new PhysicsSystem(this));

            // render should be lastish
            Systems.Add(new CameraSystem(this));
            Systems.Add(new RenderCollectorSystem(this));

            globalResources.Add(new RenderListStore());
            globalResources.Add(new InputStore());
        }

        public void UseGraphicsAdapter(IGraphicsAdapter graphics)
        {
            RenderSystems.Add(new RenderPipelineSystem(this, graphics));
        }

        public override T GetGlobalResource<T>()
        {
            foreach(var obj in globalResources)
            {
                var t = obj as T;

                if (t != null)
                    return t;
            }

            return null;
        }
    }
}
