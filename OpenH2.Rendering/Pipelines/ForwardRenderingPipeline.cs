using OpenH2.Core.Tags;
using OpenH2.Foundation;
using OpenH2.Rendering.Abstractions;
using OpenH2.Rendering.Shaders;
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

        public void Initialize() { }

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
            var passes = new RenderPasses(renderables);

            foreach(var light in pointLights)
            {
                this.adapter.AddLight(light);
            }

            this.adapter.UseShader(Shader.Skybox);
            foreach(var (skybox,xform) in passes.Skyboxes)
            {
                foreach(var mesh in skybox.Meshes)
                {
                    this.adapter.DrawMesh(mesh, xform);
                }
            }

            this.adapter.UseShader(Shader.Generic);
            foreach (var (model, xform) in passes.Diffuse)
            {
                foreach (var mesh in model.Meshes)
                {
                    this.adapter.DrawMesh(mesh, xform);
                }
            }

            renderables.Clear();
            pointLights.Clear();
        }
    }
}
