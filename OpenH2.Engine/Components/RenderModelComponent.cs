using OpenH2.Core.Architecture;
using OpenH2.Core.Extensions;
using OpenH2.Core.Tags;
using OpenH2.Foundation;
using System.Collections.Generic;
using System.Numerics;

namespace OpenH2.Engine.Components
{
    public class RenderModelComponent : Component
    {
        public Vector3 Position { get; set; } = Vector3.Zero;

        public Quaternion Orientation { get; set; } = Vector3.Zero.ToQuaternion();

        public Vector3 Scale { get; set; } = Vector3.One;

        public Mesh[] Meshes { get; set; } = new Mesh[0];

        public Dictionary<uint, IMaterial<BitmapTag>> Materials { get; set; } = new Dictionary<uint, IMaterial<BitmapTag>>();

        public string Note { get; set; }

        public RenderModelComponent(Entity parent) : base(parent)
        {
        }
    }
}
