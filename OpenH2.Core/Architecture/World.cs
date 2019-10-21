using System.Collections.Generic;

namespace OpenH2.Core.Architecture
{
    public abstract class World
    {
        public Scene Scene { get; private set; }

        public List<WorldSystem> Systems { get; private set; } = new List<WorldSystem>();

        public virtual void LoadScene(Scene scene)
        {
            this.Scene = scene;
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
    }
}
