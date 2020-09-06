using OpenH2.Core.Tags;
using OpenH2.Rendering.Abstractions;
using OpenH2.Rendering.Shaders;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using PointLight = OpenH2.Foundation.PointLight;

namespace OpenH2.Rendering.Pipelines
{
    public class ForwardRenderingPipeline : IRenderingPipeline<BitmapTag>
    {
        private readonly IGraphicsAdapter adapter;

        private IList<DrawGroup> renderables = new List<DrawGroup>();
        private List<PointLight> pointLights = new List<PointLight>();

        public ForwardRenderingPipeline(IGraphicsAdapter graphicsAdapter)
        {
            this.adapter = graphicsAdapter;
        }

        public void SetModels(IList<DrawGroup> models)
        {
            renderables = models;
        }

        public void AddPointLight(PointLight light)
        {
            this.pointLights.Add(light);
        }

        private Stopwatch drawElapsed = new Stopwatch();

        /// <summary>
        /// Kicks off the draw calls to the graphics adapter
        /// </summary>
        public void DrawAndFlush()
        {
            drawElapsed.Restart();

            foreach(var light in pointLights)
            {
                this.adapter.AddLight(light);
            }

            this.adapter.UseShader(Shader.Skybox);
            for (var i = 0; i < renderables.Count; i++)
            {
                var renderable = renderables[i];
                if (RenderPasses.IsSkybox(renderable))
                {
                    this.adapter.UseTransform(renderable.Transform);

                    this.adapter.DrawMeshes(renderable.DrawCommands);
                }
            }

            //// PERF: add in sorted order to prevent enumerating renderables multiple times
            this.adapter.UseShader(Shader.Generic);
            for (var i = 0; i < renderables.Count; i++)
            {
                var renderable = renderables[i];
                if (RenderPasses.IsDiffuse(renderable))
                {
                    this.adapter.UseTransform(renderable.Transform);
            
                    this.adapter.DrawMeshes(renderable.DrawCommands);
                }
            }

            this.adapter.UseShader(Shader.Wireframe);
            for (var i = 0; i < renderables.Count; i++)
            {
                var renderable = renderables[i];
                if (RenderPasses.IsWireframe(renderable))
                {
                    this.adapter.UseTransform(renderable.Transform);

                    this.adapter.DrawMeshes(renderable.DrawCommands);
                }
            }

            pointLights.Clear();

            drawElapsed.Stop();

            //Console.WriteLine("RenderTime: " + drawElapsed.ElapsedMilliseconds + "ms");
        }
    }
}
