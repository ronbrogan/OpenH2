using OpenH2.Engine.Components;
using OpenH2.Foundation;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenH2.Engine.Stores
{
    public class RenderListStore
    {
        private List<Mesh> meshes = new List<Mesh>();


        public void Clear()
        {
            meshes.Clear();
        }

        public void AddRenderModel(RenderModelComponent component)
        {
            foreach(var mesh in component.Meshes)
            {
                meshes.Add(mesh);
            }
        }

        public Mesh[] Meshes()
        {
            return meshes.ToArray();
        }
    }
}
