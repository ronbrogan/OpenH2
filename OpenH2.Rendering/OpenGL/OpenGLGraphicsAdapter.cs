using OpenH2.Foundation;
using OpenH2.Rendering.Abstractions;
using OpenH2.Rendering.Shaders;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace OpenH2.Rendering.OpenGL
{
    public class OpenGLGraphicsAdapter : IGraphicsAdapter
    {
        private Dictionary<Mesh, uint> meshLookup = new Dictionary<Mesh, uint>();
        private int? defaultShader;
        int MatriciesUniformHandle;
        private MatriciesUniform MatriciesUniform;

        public OpenGLGraphicsAdapter()
        {
        }

        public void UseMatricies(MatriciesUniform matricies)
        {
            MatriciesUniform = matricies;
        }

        public void UploadMesh(Mesh mesh)
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
        }

        public void DrawMesh(Mesh mesh)
        {
            if (defaultShader.HasValue == false)
            {
                defaultShader = ShaderCompiler.CreateStandardShader();
            }

            GL.UseProgram(defaultShader.Value);

            if (meshLookup.ContainsKey(mesh) == false)
            {
                UploadMesh(mesh);
            }

            SetupMatrixUniform();

            // TODO shaders, transforms
            
            

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

        private void BindMesh(Mesh mesh)
        {
            GL.BindVertexArray(meshLookup[mesh]);
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

            GL.BufferData(BufferTarget.UniformBuffer, MatriciesUniform.Size, ref MatriciesUniform, BufferUsageHint.DynamicDraw);

            GL.BindBufferBase(BufferRangeTarget.UniformBuffer, 0, MatriciesUniformHandle);
            GL.BindBuffer(BufferTarget.UniformBuffer, 0);
        }
    }
}
