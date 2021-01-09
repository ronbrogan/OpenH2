using OpenH2.Core.Architecture;
using OpenH2.Engine.Entities;
using System.Collections.Generic;

namespace OpenH2.Engine.Systems
{
    public class ActorSystem : WorldSystem
    {
        private List<Player> players = new();
        public Player[] Players { get; private set; }

        public ActorSystem(World world) : base(world)
        {
        }

        public override void Initialize(Scene scene)
        {
            scene.OnEntityAdd += this.Scene_OnEntityAdd;
            scene.OnEntityRemove += this.Scene_OnEntityRemove;
        }

        private void Scene_OnEntityRemove(Entity entity)
        {
            if (entity is Player p && players.Contains(p))
            {
                players.Remove(p);
                Players = players.ToArray();
            }
        }

        private void Scene_OnEntityAdd(Entity entity)
        {
            if(entity is Player p)
            {
                players.Add(p);
                Players = players.ToArray();
            }
        }

        public override void Update(double timestep)
        {
        }
    }
}
