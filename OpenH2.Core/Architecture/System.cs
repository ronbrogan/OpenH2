namespace OpenH2.Core.Architecture
{
    public abstract class WorldSystem
    {
        protected World world;

        public WorldSystem(World world)
        {
            this.world = world;
        }

        public abstract void Update(double timestep);
    }
}
