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

        /// <summary>
        /// Called when the system should update any cached values from the world
        /// TODO: hookup to some sort of callback/listener system
        /// </summary>
        public virtual void Initialize() { }
    }
}
