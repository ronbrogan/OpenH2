using OpenH2.Core.Architecture;
using OpenH2.Core.Factories;
using OpenH2.Core.Tags;
using OpenH2.Foundation;
using System.Numerics;

namespace OpenH2.Engine.Components
{
    public class BoundsComponent : Component
    {
        public Vector3 Least { get; set; }
        public Vector3 Most { get; set; }

        public Model<BitmapTag> RenderModel { get; set; }

        public BoundsComponent(Entity parent, Vector3 least, Vector3 most) : this(parent, least, most, new Vector4(1, 0, 0, 1))
        {
        }

        public BoundsComponent(Entity parent, Vector3 least, Vector3 most, Vector4 color) : base(parent)
        {
            Least = least;
            Most = most;
            RenderModel = ModelFactory.Cuboid(least, most, color);
            RenderModel.Flags = ModelFlags.Wireframe;
        }
    }
}
