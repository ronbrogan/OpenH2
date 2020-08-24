using OpenH2.Core.Architecture;
using OpenH2.Core.Tags;
using OpenH2.Engine.Components;
using OpenH2.Foundation;
using OpenH2.Rendering;
using System.Collections.Generic;
using System.Numerics;

namespace OpenH2.Engine.Stores
{
    /// <summary>
    /// RenderListStore is responsible for storing the objects that need to be passed to the RenderingPipeline
    /// </summary>
    public class RenderListStore
    {
        public List<DrawGroup> Models { get; } = new List<DrawGroup>();
        public List<VertexFormat> Points { get; } = new List<VertexFormat>();
        public List<PointLight> Lights { get; } = new List<PointLight>();

        public void Clear()
        {
            Models.Clear();
            Lights.Clear();
            Points.Clear();
        }

        public void Add(DrawGroup group)
        {
            this.Models.Add(group);
        }

        public void Add(VertexFormat point)
        {
            this.Points.Add(point);
        }

        public void Add(PointLight light)
        {
            this.Lights.Add(light);
        }
    }
}
