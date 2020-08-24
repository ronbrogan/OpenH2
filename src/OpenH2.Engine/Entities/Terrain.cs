using OpenH2.Core.Architecture;

namespace OpenH2.Engine.Entities
{
    public class Terrain : Entity
    {
        public Terrain()
        {
        }

        public void SetComponents(Component[] components)
        {
            this.Components = components;
        }
    }
}
