using OpenH2.Core.Extensions;
using OpenH2.Core.Tags;
using OpenH2.Foundation;
using OpenH2.Rendering.Abstractions;
using OpenH2.Rendering.Shaders;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace OpenH2.Rendering
{
    public class ForwardRenderingPipeline : IRenderingPipeline<BitmapTag>
    {
        private readonly IGraphicsAdapter adapter;

        private List<Model<BitmapTag>> renderables = new List<Model<BitmapTag>>();

        // Position, color, intensity
        private List<(Vector3, Vector3, float)> pointLights = new List<(Vector3, Vector3, float)>();

        public ForwardRenderingPipeline(IGraphicsAdapter graphicsAdapter)
        {
            this.adapter = graphicsAdapter;
        }


        /// <summary>
        /// Should be called for each object that to be drawn each frame
        /// </summary>
        /// <param name="meshes"></param>
        public void AddStaticModel(Model<BitmapTag> model)
        {
            renderables.Add(model);
        }

        public void AddTerrain(ScenarioTag.Terrain terrain)
        {
            
        }

        public void AddSkybox(object skybox)
        {
            
        }

        public void AddPointLight(Vector3 position, Vector3 color, float intensity)
        {
            this.pointLights.Add((position, color, intensity));
        }

        /// <summary>
        /// Kicks off the draw calls to the graphics adapter
        /// </summary>
        public void DrawAndFlush()
        {
            var passes = new RenderPasses(renderables);

            this.adapter.UseShader(Shader.Skybox);
            foreach(var skybox in passes.Skyboxes)
            {
                var xform = skybox.CreateTransformationMatrix();
                foreach(var mesh in skybox.Meshes)
                {
                    this.adapter.DrawMesh(mesh, xform);
                }
            }

            this.adapter.UseShader(Shader.Generic);
            foreach (var model in passes.Diffuse)
            {
                var xform = model.CreateTransformationMatrix();
                foreach (var mesh in model.Meshes)
                {
                    this.adapter.DrawMesh(mesh, xform);
                }
            }

            renderables.Clear();
            pointLights.Clear();
        }

        private class RenderPasses
        {
            public RenderPasses(ICollection<Model<BitmapTag>> renderables)
            {
                Diffuse = new List<Model<BitmapTag>>(renderables.Count);
                ShadowInteractables = new List<Model<BitmapTag>>(renderables.Count);

                foreach(var renderable in renderables)
                {
                    // TODO figure out why this can be null
                    if (renderable == null)
                        continue;

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
            public List<Model<BitmapTag>> Skyboxes = new List<Model<BitmapTag>>();
            public List<Model<BitmapTag>> Transparent = new List<Model<BitmapTag>>();
            public List<Model<BitmapTag>> ShadowInteractables;
            public List<Model<BitmapTag>> Diffuse;
        }
    }
}
