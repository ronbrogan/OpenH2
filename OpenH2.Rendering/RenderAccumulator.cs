using OpenH2.Core.Tags;
using OpenH2.Foundation;
using OpenH2.Rendering.Abstractions;
using OpenH2.Rendering.Shaders;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace OpenH2.Rendering
{
    public class RenderAccumulator : IRenderAccumulator<BitmapTag>
    {
        private readonly IGraphicsAdapter adapter;

        private Dictionary<Guid, Renderable> renderables = new Dictionary<Guid, Renderable>();

        public RenderAccumulator(IGraphicsAdapter graphicsAdapter)
        {
            this.adapter = graphicsAdapter;
        }


        /// <summary>
        /// Should be called for each object that to be drawn each frame
        /// </summary>
        /// <param name="meshes"></param>
        public void AddRigidBody(Mesh mesh, IMaterial<BitmapTag> mat, Matrix4x4 transform, ModelFlags flags)
        {
            var id = Guid.NewGuid();

            var renderable = new Renderable()
            {
                Material = mat,
                Mesh = mesh,
                Transform = transform,
                Flags = flags
            };

            renderables[id] = renderable;
        }

        public void AddTerrain(ScenarioTag.Terrain terrain)
        {
            
        }

        public void AddSkybox(object skybox)
        {
            
        }

        /// <summary>
        /// Kicks off the draw calls to the graphics adapter
        /// </summary>
        public void DrawAndFlush()
        {
            var passes = new RenderPasses(renderables.Values);

            this.adapter.UseShader(Shader.Skybox);
            foreach(var skybox in passes.Skyboxes)
            {
                this.adapter.DrawMesh(skybox.Mesh, skybox.Material, skybox.Transform);
            }

            this.adapter.UseShader(Shader.Generic);
            foreach (var model in passes.Diffuse)
            {
                this.adapter.DrawMesh(model.Mesh, model.Material, model.Transform);
            }

            renderables.Clear();
        }

        private class RenderPasses
        {
            public RenderPasses(ICollection<Renderable> renderables)
            {
                Diffuse = new List<Renderable>(renderables.Count);
                ShadowInteractables = new List<Renderable>(renderables.Count);

                foreach(var renderable in renderables)
                {
                    if(renderable.Flags.HasFlag(ModelFlags.IsSkybox))
                    {
                        Skyboxes.Add(renderable);
                    }

                    if (renderable.Flags.HasFlag(ModelFlags.CastsShadows) || renderable.Flags.HasFlag(ModelFlags.ReceivesShadows))
                    {
                        ShadowInteractables.Add(renderable);
                    }

                    if (renderable.Flags.HasFlag(ModelFlags.Diffuse))
                    {
                        Diffuse.Add(renderable);
                    }

                    if (renderable.Flags.HasFlag(ModelFlags.IsTransparent))
                    {
                        Transparent.Add(renderable);
                    }
                }
            }

            // Assume small amount of Skybox and Transparent objects
            public List<Renderable> Skyboxes = new List<Renderable>();
            public List<Renderable> Transparent = new List<Renderable>();
            public List<Renderable> ShadowInteractables;
            public List<Renderable> Diffuse;
        }



        private class Renderable
        {
            public Mesh Mesh;
            public IMaterial<BitmapTag> Material;
            public Matrix4x4 Transform;
            public ModelFlags Flags;
        }
    }
}
