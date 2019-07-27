using System.Collections.Generic;
using OpenH2.Core.Architecture;
using OpenH2.Core.Tags;
using OpenH2.Foundation;
using OpenH2.Rendering.Abstractions;

namespace OpenH2.Rendering
{
    public class RenderAccumulator : IRenderAccumulator
    {
        private readonly IGraphicsAdapter adapter;
        private Dictionary<uint, List<Mesh>> meshesByShader = new Dictionary<uint, List<Mesh>>();

        public RenderAccumulator(IGraphicsAdapter graphicsAdapter)
        {
            this.adapter = graphicsAdapter;
        }


        /// <summary>
        /// Should be called for each object that to be drawn each frame
        /// </summary>
        /// <param name="meshes"></param>
        public void AddRigidBody(Mesh mesh)
        {
            var shader = default(uint);

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
                    this.adapter.DrawMesh(mesh);
                }
            }

            meshesByShader.Clear();
        }
    }
}
