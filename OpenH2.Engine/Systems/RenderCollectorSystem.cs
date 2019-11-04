using OpenH2.Core.Architecture;
using OpenH2.Engine.Stores;

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

            var entities = this.world.Scene.Entities.Values;
            foreach(var entity in entities)
            {
                renderList.AddEntity(entity);
            }
        }
    }
}
