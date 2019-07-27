using OpenH2.Core.Architecture;
using OpenH2.Foundation;
using System.Collections.Generic;

namespace OpenH2.Engine.Components
{
    public class RenderModelComponent : Component
    {
        public List<Mesh> Meshes { get; set; }

        public RenderModelComponent(Entity parent) : base(parent)
        {
        }
    }
}
