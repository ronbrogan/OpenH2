using OpenH2.Core.Architecture;
using OpenH2.Core.Tags;
using OpenH2.Foundation;
using System.Collections.Generic;
using System.Numerics;

namespace OpenH2.Engine.Components
{
    public class RenderModelComponent : Component
    {
        public Vector3 Position { get; set; } = Vector3.Zero;

        public Vector3 Orientation { get; set; } = Vector3.Zero;

        public Mesh[] Meshes { get; set; } = new Mesh[0];

        public Dictionary<uint, IMaterial<BitmapTag>> Materials { get; set; } = new Dictionary<uint, IMaterial<BitmapTag>>();

        public RenderModelComponent(Entity parent) : base(parent)
        {
        }
    }
}
