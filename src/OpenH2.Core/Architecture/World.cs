using OpenH2.Foundation.Physics;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

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

            Parallel.ForEach(Systems, s => s.Initialize(scene));
        }

        public List<T> Components<T>() where T : Component
        {
            if (Scene == null)
                return null;

            var accum = new List<T>();

            foreach (var entity in this.Scene.Entities.Values)
            {
                if(entity.TryGetChild<T>(out var c))
                {
                    accum.Add(c);
                }
            }

            return accum;
        }

        public List<T> Entities<T>() where T : Entity
        {
            if (Scene == null)
                return null;

            var accum = new List<T>();

            foreach (var entity in this.Scene.Entities.Values)
            {
                if (entity is T tentity)
                {
                    accum.Add(tentity);
                }
            }

            return accum;
        }

        public abstract T GetGlobalResource<T>() where T : class;

        public void Update(double timestep)
        {
            this.Scene.ProcessUpdates();

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
