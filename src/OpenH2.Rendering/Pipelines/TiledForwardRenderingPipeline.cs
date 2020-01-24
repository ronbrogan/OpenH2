using OpenH2.Core.Tags;
using OpenH2.Core.Tags.Scenario;
using OpenH2.Foundation;
using OpenH2.Rendering.Abstractions;
using OpenH2.Rendering.Shaders;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace OpenH2.Rendering.Pipelines
{
    public class TiledForwardRenderingPipeline : IRenderingPipeline<BitmapTag>
    {
        private readonly IGraphicsAdapter adapter;

        private IList<(Model<BitmapTag>, Matrix4x4)> renderables = new List<(Model<BitmapTag>, Matrix4x4)>();

        public TiledForwardRenderingPipeline(IGraphicsAdapter adapter)
        {
            this.adapter = adapter;
        }

        public void AddPointLight(PointLight light)
        {
            
        }

        public void SetModels(IList<(Model<BitmapTag>, Matrix4x4)> models) => renderables = models;

        // TODO: Setup framebuffers and attachments
        // TODO: Bind framebuffers/textures(eg depth) for each shader/stage
        // TODO: Figure out shader storage binding
        public void DrawAndFlush()
        {
            //var passes = new RenderPasses(renderables);

            // Render depth
            //DrawOpaque(passes, Shader.Depth);

            // TODO: Cull lights using compute shader


            // Render diffuse
            //DrawSkybox(passes);
            //DrawOpaque(passes, Shader.Generic);


            // TODO: tonemap from HDR


            renderables.Clear();
        }


        public void AddTerrain(ScenarioTag.Terrain terrain) {}
        public void AddSkybox(object skybox) {}
    }
}
