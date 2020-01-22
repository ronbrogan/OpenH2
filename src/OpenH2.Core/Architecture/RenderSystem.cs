namespace OpenH2.Core.Architecture
{
    public abstract class RenderSystem
    {
        protected World world;

        public RenderSystem(World world)
        {
            this.world = world;
        }

        public abstract void Render(double timestep);
    }
}
