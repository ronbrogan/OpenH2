using OpenH2.Core.Tags;
using OpenH2.Engine.Components;
using OpenH2.Foundation;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenH2.Engine.Stores
{
    public class RenderListStore
    {
        public Dictionary<uint, IMaterial<Bitmap>> Materials = new Dictionary<uint, IMaterial<Bitmap>>();
        public List<Foundation.Model> Models = new List<Foundation.Model>();


        public void Clear()
        {
            Models.Clear();
            Materials.Clear();
        }

        public void AddRenderModel(RenderModelComponent component)
        {
            var model = new Foundation.Model
            {
                Meshes = component.Meshes,
                Position = component.Position,
                Orientation = component.Orientation
            };

            foreach (var mat in component.Materials)
            {
                Materials[mat.Key] = mat.Value;
            }

            Models.Add(model);
        }
    }
}
