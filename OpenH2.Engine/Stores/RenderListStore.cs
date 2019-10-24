using OpenH2.Core.Tags;
using OpenH2.Engine.Components;
using OpenH2.Foundation;
using System.Collections.Generic;

namespace OpenH2.Engine.Stores
{
    /// <summary>
    /// RenderListStore is responsible for storing the objects that need to be passed to the RenderingPipeline
    /// Currently, this class is responsible for translating from engine models -> rendering models
    /// TBD if this translation is necessary
    /// </summary>
    public class RenderListStore
    {
        public List<Model<BitmapTag>> Models = new List<Model<BitmapTag>>();
        public List<Light> Lights = new List<Light>();

        public void Clear()
        {
            Models.Clear();
            Lights.Clear();
        }

        public void AddLight()
        {
            Lights.Add(new Light()
            {

            });
        }

        public void AddRenderModel(RenderModelComponent component)
        {
            Models.Add(component.RenderModel);
        }
    }
}
