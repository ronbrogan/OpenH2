using OpenH2.Core.Architecture;
using OpenH2.Core.Tags;
using OpenH2.Foundation;
using System.Collections.Generic;

namespace OpenH2.Engine.Components
{
    public class RenderModelComponent : Component
    {
        public List<Mesh> Meshes { get; set; } = new List<Mesh>();

        public Dictionary<int, IMaterial<Bitmap>> Materials { get; set; } = new Dictionary<int, IMaterial<Bitmap>>();

        public RenderModelComponent(Entity parent) : base(parent)
        {
        }
    }
}
