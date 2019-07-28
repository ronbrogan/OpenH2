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
        private List<(Mesh,IMaterial<Bitmap>)> meshes = new List<(Mesh, IMaterial<Bitmap>)>();


        public void Clear()
        {
            meshes.Clear();
        }

        public void AddRenderModel(RenderModelComponent component)
        {
            foreach(var mesh in component.Meshes)
            {
                meshes.Add((mesh, component.Materials[mesh.MaterialIdentifier]));
            }
        }

        public (Mesh, IMaterial<Bitmap>)[] Meshes()
        {
            return meshes.ToArray();
        }
    }
}
