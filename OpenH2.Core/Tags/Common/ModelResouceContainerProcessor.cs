using OpenH2.Core.Extensions;
using OpenH2.Foundation;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenH2.Core.Tags.Common
{
    public static class ModelResouceContainerProcessor
    {
        // TODO: figure out where this can be read from
        private static Dictionary<int, int> indiciesIndexMapping = new Dictionary<int, int>
        {
            {7, 1 },
            {8, 2 },
            {9, 2 },
            {10, 2 },
            {11, 2 },
        };


        public static Mesh[] ProcessContainer(IModelResourceContainer container, ModelShaderReference[] shaders)
        {
            if (container.Resources.Length < 4)
            {
                // TODO investigate when differing amount of resources
                // Skip if we don't have the right data setup
                return new Mesh[0];
            }

            var verts = ProcessVerticies(container);

            var partResource = container.Resources[0];
            var partData = partResource.Data.Span;
            var partCount = partData.Length / 72;

            // Process face data
            var faceResource = container.Resources[indiciesIndexMapping[container.Resources.Length]];
            var faceData = faceResource.Data.Span;

            var meshes = new List<Mesh>(partCount);

            for (var i = 0; i < partCount; i++)
            {
                var start = i * 72;

                var matId = partData.ReadUInt16At(start + 4);
                var indexStart = partData.ReadUInt16At(start + 6);
                var indexCount = partData.ReadUInt16At(start + 8);
                // TODO: Figure out where to get this value
                var elementType = container is BspTag.RenderChunk 
                    ? MeshElementType.TriangleList
                    : MeshElementType.TriangleStrip;

                var mesh = new Mesh
                {
                    Verticies = verts,
                    Indicies = new int[indexCount],
                    MaterialIdentifier = shaders[matId].ShaderId,
                    ElementType = elementType
                };

                for (var j = 0; j < indexCount; j++)
                {
                    var byteStart = (indexStart + j) * 2;

                    mesh.Indicies[j] = faceData.ReadUInt16At(byteStart);
                }

                meshes.Add(mesh);
            }

            return meshes.ToArray();
        }

        private static VertexFormat[] ProcessVerticies(IModelResourceContainer container)
        {
            int firstVertIndex = 0;
            for(var i = 0; i < container.Resources.Length; i++)
            {
                if(container.Resources[i].Type == ModelResource.ResourceType.VertexAttribute)
                {
                    firstVertIndex = i;
                    break;
                }
            }

            var verts = new VertexFormat[container.VertexCount];

            var posResouce = container.Resources[firstVertIndex];
            var posData = posResouce.Data.Span;

            for (var i = 0; i < container.VertexCount; i++)
            {
                var vert = new VertexFormat();

                vert.Position = posData.ReadVec3At(i * 12);

                verts[i] = vert;
            }

            var texResouce = container.Resources[firstVertIndex + 1];
            var texData = texResouce.Data.Span;

            for (var i = 0; i < container.VertexCount; i++)
            {
                var vert = verts[i];

                vert.TexCoords = texData.ReadVec2At(i * 8);

                verts[i] = vert;
            }

            var tbnResouce = container.Resources[firstVertIndex + 2];
            var tbnData = tbnResouce.Data.Span;

            for (var i = 0; i < container.VertexCount; i++)
            {
                var vert = verts[i];

                var start = i * 36;
                vert.Tangent = tbnData.ReadVec3At(start);
                vert.Bitangent = tbnData.ReadVec3At(start + 12);
                vert.Normal = tbnData.ReadVec3At(start + 24);

                verts[i] = vert;
            }

            return verts;
        }
    }
}
