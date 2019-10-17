using OpenH2.Core.Tags;
using OpenH2.Engine.Components;
using OpenH2.Foundation;
using System.Collections.Generic;

namespace OpenH2.Engine.Stores
{
    public class RenderListStore
    {
        public Dictionary<uint, IMaterial<BitmapTag>> Materials = new Dictionary<uint, IMaterial<BitmapTag>>();
        public List<Model> Models = new List<Model>();

        public void Clear()
        {
            Models.Clear();
            Materials.Clear();
        }

        public void AddRenderModel(RenderModelComponent component)
        {
            var model = new Model
            {
                Meshes = component.Meshes,
                Position = component.Position,
                Orientation = component.Orientation,
                Scale = component.Scale,
                Note = component.Note,
                Flags = component.Flags
            };

            foreach (var mat in component.Materials)
            {
                Materials[mat.Key] = mat.Value;
            }

            Models.Add(model);
        }
    }
}
