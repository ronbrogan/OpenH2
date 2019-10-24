using OpenH2.Core.Architecture;
using OpenH2.Core.Tags;
using OpenH2.Foundation;
using System.Collections.Generic;
using System.Numerics;

namespace OpenH2.Engine.Components
{
    public class RenderModelComponent : Component
    {
        public Model<BitmapTag> RenderModel { get; set; }

        public RenderModelComponent(Entity parent) : base(parent)
        {
        }
    }
}
