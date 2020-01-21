using OpenH2.Core.Architecture;
using OpenH2.Engine.Components;
using System.Numerics;

namespace OpenH2.Engine.Entities
{
    public class Terrain : Entity
    {
        public Terrain()
        {
            var renderComponent = new RenderModelComponent(this);

            this.Components = new Component[]
            {
                renderComponent,
                new TransformComponent(this, Vector3.Zero)
            };
        }

        public void SetComponents(Component[] components)
        {
            this.Components = components;
        }
    }
}
