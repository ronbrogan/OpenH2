using OpenH2.Core.Architecture;
using OpenH2.Engine.Stores;
using OpenH2.Engine.Systems;
using OpenH2.Rendering.Abstractions;
using OpenToolkit.Windowing.Desktop;
using System.Collections.Generic;

namespace OpenH2.Engine
{
    public class RealtimeWorld : World
    {
        private List<object> globalResources = new List<object>();

        public RealtimeWorld(Engine engine, GameWindow window)
        {
            // new up systems, order here will be order of update
            Systems.Add(new OpenTKInputSystem(this, window));
            Systems.Add(new PhysxPhysicsSystem(this));
            Systems.Add(new MoverSystem(this));
            Systems.Add(new CameraSystem(this));

            globalResources.Add(new RenderListStore());
            globalResources.Add(new InputStore());
        }

        public void UseSystem(WorldSystem system)
        {
            Systems.Add(system);
        }

        public void UseGraphicsAdapter(IGraphicsAdapter graphics)
        {
            Systems.Add(new RenderCollectorSystem(this, graphics));
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
