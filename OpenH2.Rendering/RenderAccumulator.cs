using System.Collections.Generic;
using OpenH2.Core.Architecture;
using OpenH2.Core.Tags;
using OpenH2.Foundation;
using OpenH2.Rendering.Abstractions;

namespace OpenH2.Rendering
{
    public class RenderAccumulator : IRenderAccumulator<Bitmap>
    {
        private readonly IGraphicsAdapter adapter;
        private Dictionary<uint, List<Mesh>> meshesByShader = new Dictionary<uint, List<Mesh>>();

        private Dictionary<Mesh, IMaterial<Bitmap>> materialLookups = new Dictionary<Mesh, IMaterial<Bitmap>>();

        public RenderAccumulator(IGraphicsAdapter graphicsAdapter)
        {
            this.adapter = graphicsAdapter;
        }


        /// <summary>
        /// Should be called for each object that to be drawn each frame
        /// </summary>
        /// <param name="meshes"></param>
        public void AddRigidBody(Mesh mesh, IMaterial<Bitmap> mat)
        {
            var shader = default(uint);
            materialLookups[mesh] = mat;

            if (meshesByShader.TryGetValue(shader, out var meshList))
            {
                meshList.Add(mesh);
            }
            else
            {
                meshesByShader[shader] = new List<Mesh>() { mesh };
            }
        }

        public void AddTerrain(Scenario.Terrain terrain)
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
            foreach(var shader in meshesByShader.Keys)
            {
                var meshes = meshesByShader[shader];

                foreach(var mesh in meshes)
                {
                    this.adapter.DrawMesh(mesh, materialLookups[mesh]);
                }
            }

            meshesByShader.Clear();
        }
    }
}
