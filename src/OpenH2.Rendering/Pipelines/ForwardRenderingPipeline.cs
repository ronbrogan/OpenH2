using OpenH2.Core.Extensions;
using OpenH2.Core.Tags;
using OpenH2.Core.Tags.Scenario;
using OpenH2.Foundation;
using OpenH2.Rendering.Abstractions;
using OpenH2.Rendering.Shaders;
using System;
using System.Collections.Generic;
using System.Numerics;
using PointLight = OpenH2.Foundation.PointLight;

namespace OpenH2.Rendering.Pipelines
{
    public class ForwardRenderingPipeline : IRenderingPipeline<BitmapTag>
    {
        private readonly IGraphicsAdapter adapter;

        private List<(Model<BitmapTag>, Matrix4x4)> renderables = new List<(Model<BitmapTag>, Matrix4x4)>();
        private List<PointLight> pointLights = new List<PointLight>();

        public ForwardRenderingPipeline(IGraphicsAdapter graphicsAdapter)
        {
            this.adapter = graphicsAdapter;
        }


        /// <summary>
        /// Should be called for each object that to be drawn each frame
        /// </summary>
        /// <param name="meshes"></param>
        public void AddStaticModel(Model<BitmapTag> model, Matrix4x4 transform)
        {
            renderables.Add((model, transform));
        }

        public void AddTerrain(ScenarioTag.Terrain terrain)
        {
        }

        public void AddSkybox(object skybox)
        {
        }

        public void AddPointLight(PointLight light)
        {
            this.pointLights.Add(light);
        }

        /// <summary>
        /// Kicks off the draw calls to the graphics adapter
        /// </summary>
        public void DrawAndFlush()
        {
            foreach(var light in pointLights)
            {
                this.adapter.AddLight(light);
            }

            this.adapter.UseShader(Shader.Skybox);
            for (var i = 0; i < renderables.Count; i++)
            {
                var renderable = renderables[i];
                if(RenderPasses.IsSkybox(renderable.Item1))
                {
                    foreach (var mesh in renderable.Item1.Meshes)
                    {
                        this.adapter.DrawMesh(mesh, renderable.Item2);
                    }
                }
            }

            // PERF: add in sorted order to prevent enumerating renderables multiple times
            this.adapter.UseShader(Shader.Generic);
            for (var i = 0; i < renderables.Count; i++)
            {
                var renderable = renderables[i];
                if (RenderPasses.IsDiffuse(renderable.Item1))
                {
                    foreach (var mesh in renderable.Item1.Meshes)
                    {
                        this.adapter.DrawMesh(mesh, renderable.Item2);
                    }
                }
            }

            this.adapter.UseShader(Shader.Wireframe);
            for (var i = 0; i < renderables.Count; i++)
            {
                var renderable = renderables[i];
                if (RenderPasses.IsWireframe(renderable.Item1))
                {
                    foreach (var mesh in renderable.Item1.Meshes)
                    {
                        this.adapter.DrawMesh(mesh, renderable.Item2);
                    }
                }
            }

            renderables.Clear();
            pointLights.Clear();
        }
    }
}
