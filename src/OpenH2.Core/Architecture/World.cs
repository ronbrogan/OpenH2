using OpenH2.Foundation.Physics;
using System.Collections.Generic;
using System.Numerics;

namespace OpenH2.Core.Architecture
{
    public abstract class World : IPhysicsWorld
    {
        public Scene Scene { get; private set; }

        public List<WorldSystem> Systems { get; private set; } = new List<WorldSystem>();
        public List<RenderSystem> RenderSystems { get; private set; } = new List<RenderSystem>();

        public Vector3 Gravity { get; set; } = new Vector3(0, 0, -9.8f);

        public virtual void LoadScene(Scene scene)
        {
            this.Scene = scene;

            foreach(var system in Systems)
            {
                system.Initialize();
            }
        }

        public List<T> Components<T>() where T : Component
        {
            if (Scene == null)
                return null;

            var accum = new List<T>();

            foreach(var entity in this.Scene.Entities.Values)
            {
                if(entity.TryGetChild<T>(out var c))
                {
                    accum.Add(c);
                }
            }

            return accum;
        }

        public abstract T GetGlobalResource<T>() where T : class;

        public void Update(double timestep)
        {
            foreach (var system in Systems)
            {
                system.Update(timestep);
            }
        }

        public void Render(double timestep)
        {
            foreach (var system in RenderSystems)
            {
                system.Render(timestep);
            }
        }
    }
}
