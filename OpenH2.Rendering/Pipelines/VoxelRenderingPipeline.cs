using OpenH2.Core.Tags;
using OpenH2.Foundation;
using OpenH2.Rendering.Abstractions;
using OpenH2.Rendering.Shaders;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using OpenTK.Graphics.OpenGL;
using OpenH2.Rendering.Shaders.Voxelize;

namespace OpenH2.Rendering.Pipelines
{
    public class VoxelRenderingPipeline : IRenderingPipeline<BitmapTag>
    {
        private readonly IGraphicsAdapter adapter;

        private List<(Model<BitmapTag>, Matrix4x4)> renderables = new List<(Model<BitmapTag>, Matrix4x4)>();
        private List<PointLight> pointLights = new List<PointLight>();

        private int voxelUniformHandle;
        private const int voxelTextureSize = 64;
        private float[] voxelTextureData = new float[voxelTextureSize * voxelTextureSize * voxelTextureSize * sizeof(float)];
        private long voxelTextureHandle;

        public VoxelRenderingPipeline(IGraphicsAdapter graphicsAdapter)
        {
            this.adapter = graphicsAdapter;
        }

        public void Initialize() 
        {
            var texId = this.adapter.TextureBinder.Bind3D(voxelTextureData,
                    voxelTextureSize,
                    voxelTextureSize,
                    voxelTextureSize,
                    true,
                    out voxelTextureHandle);

            this.adapter.SetupShaderBegin(Shader.Voxelize, a =>
            {
                GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                GL.Viewport(0, 0, voxelTextureSize, voxelTextureSize);
                GL.Disable(EnableCap.CullFace);
                GL.Disable(EnableCap.DepthTest);
                GL.Disable(EnableCap.Blend);
                GL.ColorMask(false, false, false, false);

                // Bind texture
                // TODO evaluate bindless writes?
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture3D, texId);
                GL.BindImageTexture(0, texId, 0, true, 0, TextureAccess.WriteOnly, SizedInternalFormat.Rgba8);
            });

            this.adapter.SetupShaderEnd(Shader.Voxelize, adapter =>
            {
                GL.GenerateMipmap(GenerateMipmapTarget.Texture3D);
                GL.ColorMask(true, true, true, true);

                GL.Viewport(0, 0, 1600, 900);
            });

            this.adapter.SetupShaderBegin(Shader.VoxelWorldPositioning, a =>
            {
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture3D, texId);
                GL.BindImageTexture(0, texId, 0, true, 0, TextureAccess.WriteOnly, SizedInternalFormat.Rgba8);
            });

            this.adapter.SetupShaderBegin(Shader.VoxelVisualizer, a =>
            {
                GL.ActiveTexture(TextureUnit.Texture0);
                GL.BindTexture(TextureTarget.Texture3D, texId);
                GL.BindImageTexture(0, texId, 0, true, 0, TextureAccess.WriteOnly, SizedInternalFormat.Rgba8);
            });
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
            var passes = new RenderPasses(renderables);

            foreach (var light in pointLights)
            {
                this.adapter.AddLight(light);
            }

            this.adapter.UseShader(Shader.Voxelize);
            foreach (var (model, xform) in passes.Diffuse)
            {
                foreach (var mesh in model.Meshes)
                {
                    this.adapter.DrawMesh(mesh, xform);
                }
            }

            var doVisualization = true;

            if(doVisualization)
            {
                DrawVoxelVisualization(passes);
            }
            else
            {
                DrawLighting(passes);
            }

            renderables.Clear();
            pointLights.Clear();
        }

        private void DrawVoxelVisualization(RenderPasses passes)
        {
            //this.adapter.UseShader(Shader.VoxelWorldPositioning);
            // TODO: need a way to pass data between passes that is used by the adapter


            this.adapter.UseShader(Shader.Generic);
            foreach (var (model, xform) in passes.Diffuse)
            {
                foreach (var mesh in model.Meshes)
                {
                    this.adapter.DrawMesh(mesh, xform);
                }
            }
        }

        private void DrawLighting(RenderPasses passes)
        {
            this.adapter.UseShader(Shader.Skybox);
            foreach (var (skybox, xform) in passes.Skyboxes)
            {
                foreach (var mesh in skybox.Meshes)
                {
                    this.adapter.DrawMesh(mesh, xform);
                }
            }
        }
    }
}
