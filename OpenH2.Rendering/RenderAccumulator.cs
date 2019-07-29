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
        private Dictionary<IMaterial<BitmapTag>, List<Mesh>> meshesByMaterial = new Dictionary<IMaterial<BitmapTag>, List<Mesh>>();

        private Dictionary<Mesh, IMaterial<BitmapTag>> materials = new Dictionary<Mesh, IMaterial<BitmapTag>>();
        private Dictionary<Mesh, Matrix4x4> transforms = new Dictionary<Mesh, Matrix4x4>();

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
            materials[mesh] = mat;
            transforms[mesh] = transform;

            if (meshesByMaterial.TryGetValue(mat, out var meshList))
            {
                meshList.Add(mesh);
            }
            else
            {
                meshesByMaterial[mat] = new List<Mesh>() { mesh };
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
            foreach(var material in meshesByMaterial.Keys)
            {
                var meshes = meshesByMaterial[material];

                foreach(var mesh in meshes)
                {
                    var xform = transforms[mesh];

                    this.adapter.DrawMesh(mesh, material, xform);
                }
            }

            meshesByMaterial.Clear();
        }
    }
}
