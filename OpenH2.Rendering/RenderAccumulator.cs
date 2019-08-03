using System;
using System.Collections.Generic;
using System.Numerics;
using OpenH2.Core.Architecture;
using OpenH2.Core.Tags;
using OpenH2.Foundation;
using OpenH2.Rendering.Abstractions;

namespace OpenH2.Rendering
{
    public class RenderAccumulator : IRenderAccumulator<BitmapTag>
    {
        private readonly IGraphicsAdapter adapter;
        private Dictionary<IMaterial<BitmapTag>, List<Guid>> renderablesByMaterial = new Dictionary<IMaterial<BitmapTag>, List<Guid>>();

        private Dictionary<Guid, Renderable> renderables = new Dictionary<Guid, Renderable>();

        public RenderAccumulator(IGraphicsAdapter graphicsAdapter)
        {
            this.adapter = graphicsAdapter;
        }


        /// <summary>
        /// Should be called for each object that to be drawn each frame
        /// </summary>
        /// <param name="meshes"></param>
        public void AddRigidBody(Mesh mesh, IMaterial<BitmapTag> mat, Matrix4x4 transform)
        {
            var id = Guid.NewGuid();

            var renderable = new Renderable()
            {
                Material = mat,
                Mesh = mesh,
                Transform = transform
            };

            renderables[id] = renderable;

            if (renderablesByMaterial.TryGetValue(mat, out var meshList))
            {
                meshList.Add(id);
            }
            else
            {
                renderablesByMaterial[mat] = new List<Guid>() { id };
            }
        }

        public void AddTerrain(ScenarioTag.Terrain terrain)
        {
            
        }

        public void AddSkybox(object skybox)
        {
            
        }

        /// <summary>
        /// Kicks of the draw calls to the graphics adapter
        /// </summary>
        public void DrawAndFlush()
        {
            foreach(var mat in renderablesByMaterial.Keys)
            {
                var ids = renderablesByMaterial[mat];

                foreach(var id in ids)
                {
                    var renderable = renderables[id];

                    this.adapter.DrawMesh(renderable.Mesh, renderable.Material, renderable.Transform);
                }
            }

            renderablesByMaterial.Clear();
            renderables.Clear();
        }

        private class Renderable
        {
            public Mesh Mesh;
            public IMaterial<BitmapTag> Material;
            public Matrix4x4 Transform;
        }
    }
}
