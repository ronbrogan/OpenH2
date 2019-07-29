using OpenH2.Core.Tags;
using OpenH2.Foundation;
using OpenH2.Rendering.Abstractions;
using OpenH2.Rendering.Shaders;
using OpenH2.Rendering.Shaders.Generic;
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
        private int? defaultShader;
        int MatriciesUniformHandle;
        int GenericUniformHandle;
        private GlobalUniform MatriciesUniform;

        public OpenGLGraphicsAdapter()
        {
        }

        public void UseMatricies(GlobalUniform matricies)
        {
            MatriciesUniform = matricies;
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

        public void SetupTextures(IMaterial<BitmapTag> material)
        {
            if (boundTextures.Contains(material))
                return;

            if(material.DiffuseMap != null)
            {
                textureBinder.Bind(material.DiffuseMap, out var diffuseHandle);
                material.DiffuseHandle = diffuseHandle;
            }

            boundTextures.Add(material);
        }

        // PERF: sort calls by material and vao and deduplicate GL calls 
        public void DrawMesh(Mesh mesh, IMaterial<BitmapTag> material, Matrix4x4 transform)
        {
            SetupTextures(material);
            UseGenericShader();
            SetupMatrixUniform();

            if (Matrix4x4.Invert(transform, out var inverted) == false)
            {
                throw new Exception("Couldn't invert model matrix");
            }
         
            var genericUniform = new GenericUniform()
            {
                ModelMatrix = transform,
                NormalMatrix = Matrix4x4.Transpose(inverted),
                DiffuseColor = new Vector4(material.DiffuseColor, 1),
                UseDiffuseMap = material.DiffuseHandle != default,
                DiffuseMap = material.DiffuseHandle,
                DiffuseAmount = 1f
            };

            SetupGenericUniform(genericUniform);

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
                case MeshElementType.PolygonList:
                    GL.DrawElements(PrimitiveType.Polygon, indicies.Length, DrawElementsType.UnsignedInt, 0);
                    break;
            }
        }

        private void UseGenericShader()
        {
            if (defaultShader.HasValue == false)
            {
                defaultShader = ShaderCompiler.CreateStandardShader();
            }

            GL.UseProgram(defaultShader.Value);
        }

        private void BindMesh(Mesh mesh)
        {
            if (meshLookup.TryGetValue(mesh, out var vaoId) == false)
            {
                vaoId = UploadMesh(mesh);
            }

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

        private void SetupMatrixUniform()
        {
            
            if(MatriciesUniformHandle == default(int))
                GL.GenBuffers(1, out MatriciesUniformHandle);

            GL.BindBuffer(BufferTarget.UniformBuffer, MatriciesUniformHandle);

            GL.BufferData(BufferTarget.UniformBuffer, GlobalUniform.Size, ref MatriciesUniform, BufferUsageHint.DynamicDraw);

            GL.BindBufferBase(BufferRangeTarget.UniformBuffer, 0, MatriciesUniformHandle);
            GL.BindBuffer(BufferTarget.UniformBuffer, 0);
        }

        private void SetupGenericUniform(GenericUniform uniform)
        {
            if (GenericUniformHandle == default(int))
            {
                GL.GenBuffers(1, out GenericUniformHandle);
                GL.BindBuffer(BufferTarget.UniformBuffer, GenericUniformHandle);
                GL.BufferData(BufferTarget.UniformBuffer, GenericUniform.Size, IntPtr.Zero, BufferUsageHint.DynamicDraw);
            }
            else
            {
                GL.BindBuffer(BufferTarget.UniformBuffer, GenericUniformHandle);
            }

            GL.BufferSubData(BufferTarget.UniformBuffer, IntPtr.Zero, GenericUniform.Size, ref uniform);

            GL.BindBufferBase(BufferRangeTarget.UniformBuffer, 1, GenericUniformHandle);
            GL.BindBuffer(BufferTarget.UniformBuffer, 0);
        }
    }
}
