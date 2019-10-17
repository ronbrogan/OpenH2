﻿using OpenH2.Core.Tags;
using OpenH2.Foundation;
using OpenH2.Rendering.Abstractions;
using OpenH2.Rendering.Shaders;
using OpenH2.Rendering.Shaders.Generic;
using OpenH2.Rendering.Shaders.Skybox;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace OpenH2.Rendering.OpenGL
{
    public class OpenGLGraphicsAdapter : IGraphicsAdapter
    {
        private Dictionary<Mesh, uint> meshLookup = new Dictionary<Mesh, uint>();
        private HashSet<IMaterial<BitmapTag>> boundTextures = new HashSet<IMaterial<BitmapTag>>();
        private ITextureBinder textureBinder = new OpenGLTextureBinder();

        private Shader activeShader;
        private Dictionary<Shader, int> shaderHandles = new Dictionary<Shader, int>();
        private Dictionary<Shader, int> uniformHandles = new Dictionary<Shader, int>();

        private int GlobalUniformHandle;
        private GlobalUniform GlobalUniform;

        public OpenGLGraphicsAdapter()
        {
        }

        public void BeginFrame(GlobalUniform global)
        {
            GlobalUniform = global;
            SetupGlobalUniform();
        }

        public void UseShader(Shader shader)
        {
            if (shaderHandles.TryGetValue(shader, out var handle) == false)
            {
                handle = ShaderCompiler.CreateShader(shader);
                shaderHandles[shader] = handle;
            }

            GL.UseProgram(handle);

            activeShader = shader;

            if(shaderBeginActions.TryGetValue(shader, out var action))
            {
                action();
            }
        }

        public uint UploadMesh(Mesh mesh)
        {
            var verticies = mesh.Verticies;
            var indicies = mesh.Indicies;

            uint vao, vbo, ibo;

            GL.GenVertexArrays(1, out vao);
            GL.BindVertexArray(vao);

            GL.GenBuffers(1, out vbo);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(verticies.Length * VertexFormat.Size), verticies, BufferUsageHint.StaticDraw);

            GL.GenBuffers(1, out ibo);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indicies.Length * sizeof(uint)), indicies, BufferUsageHint.StaticDraw);

            SetupVertexFormatAttributes();

            meshLookup.Add(mesh, vao);
            return vao;
        }

        public void EndFrame()
        {
        }

        public void SetupTextures(IMaterial<BitmapTag> material)
        {
            if (boundTextures.Contains(material))
                return;

            if(material.DiffuseMap != null)
            {
                textureBinder.Bind(material.DiffuseMap, out var diffuseHandle);
                material.DiffuseHandle = diffuseHandle;
            }

            if (material.DetailMap1 != null)
            {
                textureBinder.Bind(material.DetailMap1, out var handle);
                material.Detail1Handle = handle;
            }

            if (material.DetailMap2 != null)
            {
                textureBinder.Bind(material.DetailMap2, out var handle);
                material.Detail2Handle = handle;
            }

            if (material.AlphaMap != null)
            {
                textureBinder.Bind(material.AlphaMap, out var alphaHandle);
                material.AlphaHandle = alphaHandle;
            }

            boundTextures.Add(material);
        }

        // PERF: sort calls by material and vao and deduplicate GL calls 
        public void DrawMesh(Mesh mesh, IMaterial<BitmapTag> material, Matrix4x4 transform)
        {
            SetupTextures(material);

            CreateAndBindShaderUniform(mesh, material, transform);

            BindMesh(mesh);

            var type = mesh.ElementType;
            var indicies = mesh.Indicies;

            switch (type)
            {
                case MeshElementType.TriangleList:
                    GL.DrawElements(PrimitiveType.Triangles, indicies.Length, DrawElementsType.UnsignedInt, 0);
                    break;
                case MeshElementType.TriangleStrip:
                    GL.DrawElements(PrimitiveType.TriangleStrip, indicies.Length, DrawElementsType.UnsignedInt, 0);
                    break;
                case MeshElementType.Other:
                    GL.DrawElements(PrimitiveType.TriangleStripAdjacency, indicies.Length, DrawElementsType.UnsignedInt, 0);
                    break;
                default:
                    GL.DrawElements(PrimitiveType.Triangles, indicies.Length, DrawElementsType.UnsignedInt, 0);
                    break;
            }
        }

        private void CreateAndBindShaderUniform(Mesh mesh, IMaterial<BitmapTag> material, Matrix4x4 transform)
        {
            if (Matrix4x4.Invert(transform, out var inverted) == false)
            {
                Console.WriteLine("Couldn't invert model matrix: " + mesh.Note);
                return;
            }

            switch (activeShader)
            {
                case Shader.Skybox:
                    SetupGenericUniform(
                        activeShader,
                        new SkyboxUniform(material, transform, inverted),
                        SkyboxUniform.Size);
                    break;
                case Shader.Generic:
                    SetupGenericUniform(
                        activeShader, 
                        new GenericUniform(material, transform, inverted), 
                        GenericUniform.Size);
                    break;
                case Shader.TextureViewer:
                    break;
            }
        }

        private void BindMesh(Mesh mesh)
        {
            if (meshLookup.TryGetValue(mesh, out var vaoId) == false)
            {
                vaoId = UploadMesh(mesh);
            }

            // PERF: dedupe vao bindings
            GL.BindVertexArray(vaoId);
        }

        private static void SetupVertexFormatAttributes()
        {
            // Attributes for VertexFormat.Position
            GL.EnableVertexAttribArray(0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, VertexFormat.Size, 0);

            // Attributes for VertexFormat.TexCoords
            GL.EnableVertexAttribArray(1);
            GL.VertexAttribPointer(1, 2, VertexAttribPointerType.Float, false, VertexFormat.Size, 12);

            // Attributes for VertexFormat.Normal
            GL.EnableVertexAttribArray(2);
            GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, VertexFormat.Size, 20);

            // Attributes for VertexFormat.Tangent
            GL.EnableVertexAttribArray(3);
            GL.VertexAttribPointer(3, 3, VertexAttribPointerType.Float, false, VertexFormat.Size, 32);

            // Attributes for VertexFormat.Bitangent
            GL.EnableVertexAttribArray(4);
            GL.VertexAttribPointer(4, 3, VertexAttribPointerType.Float, false, VertexFormat.Size, 44);
        }

        private void SetupGlobalUniform()
        {
            if (GlobalUniformHandle == default(int))
            {
                GL.GenBuffers(1, out GlobalUniformHandle);
                GL.BindBuffer(BufferTarget.UniformBuffer, GlobalUniformHandle);
                GL.BufferData(BufferTarget.UniformBuffer, GlobalUniform.Size, IntPtr.Zero, BufferUsageHint.DynamicDraw);
            }
            else
            {
                GL.BindBuffer(BufferTarget.UniformBuffer, GlobalUniformHandle);
            }

            GL.BufferSubData(BufferTarget.UniformBuffer, IntPtr.Zero, GlobalUniform.Size, ref GlobalUniform);

            GL.BindBufferBase(BufferRangeTarget.UniformBuffer, 0, GlobalUniformHandle);
            GL.BindBuffer(BufferTarget.UniformBuffer, 0);
        }

        private void SetupGenericUniform<T>(Shader shader, T uniform, int size) where T : struct
        {
            if (uniformHandles.TryGetValue(shader, out var handle) == false)
            {
                GL.GenBuffers(1, out handle);
                GL.BindBuffer(BufferTarget.UniformBuffer, handle);
                GL.BufferData(BufferTarget.UniformBuffer, size, IntPtr.Zero, BufferUsageHint.DynamicDraw);

                uniformHandles[shader] = handle;
            }
            else
            {
                GL.BindBuffer(BufferTarget.UniformBuffer, handle);
            }

            GL.BufferSubData(BufferTarget.UniformBuffer, IntPtr.Zero, size, ref uniform);

            GL.BindBufferBase(BufferRangeTarget.UniformBuffer, 1, handle);
            GL.BindBuffer(BufferTarget.UniformBuffer, 0);
        }

        private Dictionary<Shader, Action> shaderBeginActions = new Dictionary<Shader, Action>()
        {
            { Shader.Skybox, () => {
                GL.Disable(EnableCap.DepthTest);
            }},
            { Shader.Generic, () => {
                GL.Enable(EnableCap.DepthTest);
            }}
        };

        private Dictionary<Shader, Action> shaderEndActions = new Dictionary<Shader, Action>()
        {
            
        };
    }
}
