using OpenH2.Foundation;
using OpenH2.Rendering.Abstractions;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;

namespace OpenH2.Rendering.OpenGL
{
    public class OpenGLGraphicsAdapter : IGraphicsAdapter
    {
        private Dictionary<object, uint> meshLookup = new Dictionary<object, uint>();

        public void UploadMesh(object mesh)
        {
            // TODO use verts/indicies from mesh object
            var verticies = new List<VertexFormat>();
            var indicies = new List<int>();

            uint vao, vbo, ibo;

            GL.GenVertexArrays(1, out vao);
            GL.BindVertexArray(vao);

            GL.GenBuffers(1, out vbo);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(verticies.Count * VertexFormat.Size), verticies.ToArray(), BufferUsageHint.StaticDraw);

            GL.GenBuffers(1, out ibo);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, ibo);
            GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indicies.Count * sizeof(uint)), indicies.ToArray(), BufferUsageHint.StaticDraw);

            SetupVertexFormatAttributes();

            meshLookup.Add(mesh, vao);
        }

        public void DrawMesh(object mesh)
        {
            if(meshLookup.ContainsKey(mesh) == false)
            {
                UploadMesh(mesh);
            }

            // TODO shaders, transforms



            BindMesh(mesh);

            // TODO type and indicies from mesh object
            var type = MeshElementType.TriangleList;
            var indicies = new List<int>();

            switch (type)
            {
                case MeshElementType.TriangleList:
                    GL.DrawElements(PrimitiveType.Triangles, indicies.Count, DrawElementsType.UnsignedInt, 0);
                    break;
                case MeshElementType.TriangleStrip:
                    GL.DrawElements(PrimitiveType.TriangleStrip, indicies.Count, DrawElementsType.UnsignedInt, 0);
                    break;
                case MeshElementType.PolygonList:
                    GL.DrawElements(PrimitiveType.Polygon, indicies.Count, DrawElementsType.UnsignedInt, 0);
                    break;
            }
        }

        private void BindMesh(object mesh)
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
    }
}
