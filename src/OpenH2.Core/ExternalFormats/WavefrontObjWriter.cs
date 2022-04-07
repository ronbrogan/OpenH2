using OpenH2.Core.Tags.Common.Models;
using OpenH2.Foundation;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace OpenH2.Core.ExternalFormats
{
    /// <summary>
    /// Used to write meshes to an Obj string
    /// </summary>
    public class WavefrontObjWriter
    {
        private int vertexCount = 0;
        private StringBuilder builder;
        private Dictionary<(VertexFormat[], Matrix4x4), int> indexBasis = new Dictionary<(VertexFormat[], Matrix4x4), int>();

        public WavefrontObjWriter(StringBuilder builder, string name)
        {
            this.builder = builder;
            builder.AppendLine("g " + name);
        }

        public WavefrontObjWriter(string name)
        {
            this.builder = new StringBuilder();
            builder.AppendLine("g " + name);
        }

        public override string ToString()
        {
            return builder.ToString();
        }

        public void WriteModel(MeshCollection meshCollection, Matrix4x4 transform = default, string name = null)
        {
            var xform = transform == default ? Matrix4x4.Identity : transform;

            builder.AppendLine("o " + name ?? "mesh");

            foreach (var mesh in meshCollection.Meshes)
            {
                if(indexBasis.ContainsKey((mesh.Verticies, xform)))
                {
                    continue;
                }

                indexBasis.Add((mesh.Verticies, xform), vertexCount);

                var verts = mesh.Verticies;

                foreach (var vert in verts)
                {
                    var pos = Vector3.Transform(vert.Position, xform);

                    builder.AppendLine($"v {pos.X.ToString("0.000000")} {pos.Y.ToString("0.000000")} {pos.Z.ToString("0.000000")}");
                }

                foreach (var vert in verts)
                {
                    builder.AppendLine($"vt {vert.TexCoords.X.ToString("0.000000")} {vert.TexCoords.Y.ToString("0.000000")}");
                }

                foreach (var vert in verts)
                {
                    // TODO: transform normals
                    builder.AppendLine($"vn {vert.Normal.X.ToString("0.000000")} {vert.Normal.Y.ToString("0.000000")} {vert.Normal.Z.ToString("0.000000")}");
                }

                // Increment vertexCount 
                vertexCount += verts.Length;
            }

            // Write indices
            foreach (var mesh in meshCollection.Meshes)
            {
                var basis = indexBasis[(mesh.Verticies, xform)];

                switch (mesh.ElementType)
                {
                    case MeshElementType.TriangleStrip:
                    case MeshElementType.TriangleStripDecal:
                        WriteTriangleStrip(mesh, basis);
                        break;
                    case MeshElementType.TriangleList:
                        WriteTriangleList(mesh, basis);
                        break;
                }
            }

            
        }

        private void WriteTriangleList(ModelMesh mesh, int basis)
        {
            for (var j = 0; j < mesh.Indices.Length - 2; j+=3)
            {
                var indices = (mesh.Indices[j], mesh.Indices[j + 1], mesh.Indices[j + 2]);

                WriteTriangleIndices(indices, basis);
            }
        }

        private void WriteTriangleStrip(ModelMesh mesh, int basis)
        {
            var triangles = new List<(int, int, int)>();

            for (int i = 0; i < mesh.Indices.Length - 2; i++)
            {
                (int, int, int) triangle;

                if (i % 2 == 0)
                {
                    triangle = (
                        mesh.Indices[i],
                        mesh.Indices[i + 1],
                        mesh.Indices[i + 2]
                    );
                }
                else
                {
                    triangle = (
                        mesh.Indices[i],
                        mesh.Indices[i + 2],
                        mesh.Indices[i + 1]
                    );
                }


                triangles.Add(triangle);
            }

            foreach (var tri in triangles)
            {
                WriteTriangleIndices(tri, basis);
            }
        }
        
        private void WriteTriangleIndices((int, int, int) tri, int basis)
        {
            builder.Append("f");
            builder.Append($" {tri.Item1 + basis + 1}/{tri.Item1 + basis + 1}/{tri.Item1 + basis + 1}");
            builder.Append($" {tri.Item2 + basis + 1}/{tri.Item2 + basis + 1}/{tri.Item2 + basis + 1}");
            builder.Append($" {tri.Item3 + basis + 1}/{tri.Item3 + basis + 1}/{tri.Item3 + basis + 1}");

            builder.AppendLine("");
        }

    }
}
