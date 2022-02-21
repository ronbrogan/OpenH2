using OpenH2.Core.Tags;
using OpenH2.Rendering.Abstractions;
using OpenH2.Rendering.Shaders;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using PointLight = OpenH2.Foundation.PointLight;

namespace OpenH2.Rendering.Pipelines
{
    public class ForwardRenderingPipeline : IRenderingPipeline<BitmapTag>
    {
        private readonly IGraphicsAdapter adapter;

        private GlobalUniform Globals;
        private List<DrawGroup> renderables = new List<DrawGroup>();
        private List<(float, DrawGroup)> transparentRenderables = new List<(float, DrawGroup)>();
        private List<PointLight> pointLights = new List<PointLight>();

        public ForwardRenderingPipeline(IGraphicsAdapter graphicsAdapter)
        {
            this.adapter = graphicsAdapter;
        }

        public void SetModels(List<DrawGroup> models)
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
            transparentRenderables.Clear();

            foreach (var light in pointLights)
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

                if(RenderPasses.IsTransparent(renderable))
                {
                    this.InsertTransparentRenderable(renderable);
                }
                else if (RenderPasses.IsDiffuse(renderable))
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

            this.adapter.UseShader(Shader.Generic);
            for (var i = transparentRenderables.Count - 1; i >= 0; i--)
            {
                var renderable = transparentRenderables[i].Item2;

                this.adapter.UseTransform(renderable.Transform);

                this.adapter.DrawMeshes(renderable.DrawCommands);
            }

            pointLights.Clear();

            drawElapsed.Stop();

            //Console.WriteLine("RenderTime: " + drawElapsed.ElapsedMilliseconds + "ms");
        }

        private void InsertTransparentRenderable(DrawGroup renderable)
        {
            var distance = GetDistance(renderable);

            for (int i = 0; i < transparentRenderables.Count; i++)
            {
                var cur = transparentRenderables[i].Item1;
                
                if(distance < cur)
                {
                    transparentRenderables.Insert(i, (distance, renderable));
                    return;
                }
            }

            transparentRenderables.Add((distance, renderable));

            float GetDistance(DrawGroup renderable)
            {
                if(renderable.Transform.Translation == Vector3.Zero)
                {
                    return Vector3.DistanceSquared(this.Globals.ViewPosition, renderable.DrawCommands[0].Mesh.Verticies[0].Position);
                }

                return Vector3.DistanceSquared(this.Globals.ViewPosition, renderable.Transform.Translation);
            }
        }

        public void SetGlobals(GlobalUniform matrices)
        {
            this.Globals = matrices;
        }
    }
}
