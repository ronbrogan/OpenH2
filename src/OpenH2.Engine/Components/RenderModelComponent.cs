using OpenH2.Core.Architecture;
using OpenH2.Core.Tags;
using OpenH2.Foundation;
using OpenH2.Rendering;
using System;

namespace OpenH2.Engine.Components
{
    public class RenderModelComponent : Component
    {
        public Model<BitmapTag> RenderModel { get; }

        public DrawCommand[] DrawCommands { get; private set; }
        public ref DrawCommand this[int i] => ref this.DrawCommands[i];

        public RenderModelComponent(Entity parent, Model<BitmapTag> model) : base(parent)
        {
            this.RenderModel = model;
        }

        public void UpdateDrawCommands(DrawCommand[] commands)
        {
            this.DrawCommands = commands;
        }
    }
}
