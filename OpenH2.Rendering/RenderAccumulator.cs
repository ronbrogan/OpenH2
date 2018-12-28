using System.Collections.Generic;
using OpenH2.Core.Tags;
using OpenH2.Rendering.Abstractions;

namespace OpenH2.Rendering
{
    public class RenderAccumulator : IRenderAccumulator
    {
        private readonly IGraphicsAdapter adapter;
        private Dictionary<uint, List<object>> meshesByShader = new Dictionary<uint, List<object>>();

        public RenderAccumulator(IGraphicsAdapter graphicsAdapter)
        {
            this.adapter = graphicsAdapter;
        }

        /// <summary>
        /// Should be called for each object that to be drawn each frame
        /// </summary>
        /// <param name="meshes"></param>
        public void AddRigidBody(object model)
        {

            // TODO get meshes from model
            var meshes = new List<object>();

            foreach(var mesh in meshes)
            {
                // TODO get shader from mesh
                var shader = default(uint);

                if (meshesByShader.TryGetValue(shader, out var meshList))
                {
                    meshList.Add(mesh);
                }
                else
                {
                    meshesByShader[shader] = new List<object>() { mesh };
                }
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
