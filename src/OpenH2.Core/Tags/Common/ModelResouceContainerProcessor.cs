using OpenH2.Core.Extensions;
using OpenH2.Foundation;
using System.Collections.Generic;

namespace OpenH2.Core.Tags.Common
{
    public static class ModelResouceContainerProcessor
    {
        public static ModelMesh[] ProcessContainer(IModelResourceContainer container, ModelShaderReference[] shaders, string note = null)
        {
            if (container.Resources.Length < 4)
            {
                // TODO investigate when differing amount of resources
                // Skip if we don't have the right data setup
                return new ModelMesh[0];
            }

            var verts = ProcessVerticies(container);

            var partCount = (int)container.Header.PartInfoCount;
            var partResource = container.Resources[0];
            var partData = partResource.Data.Span;
            

            

            // Process face data
            var faceResource = container.Resources[GetIndiciesResourceIndex(container)];
            var faceData = faceResource.Data.Span;

            var meshes = new List<ModelMesh>(partCount);

            for (var i = 0; i < partCount; i++)
            {
                var start = i * 72;

                var matId = partData.ReadUInt16At(start + 4);
                var indexStart = partData.ReadUInt16At(start + 6);
                var indexCount = partData.ReadUInt16At(start + 8);
                var elementType = (MeshElementType)partData.ReadUInt16At(start + 2);

                var mesh = new ModelMesh
                {
                    Verticies = verts,
                    Indices = new int[indexCount],
                    Shader = shaders[matId].ShaderId,
                    ElementType = elementType,
                    Note = note
                };

                mesh.RawData = partData.Slice(start, 72).ToArray();

                for (var j = 0; j < indexCount; j++)
                {
                    var byteStart = (indexStart + j) * 2;

                    mesh.Indices[j] = faceData.ReadUInt16At(byteStart);
                }

                meshes.Add(mesh);
            }

            return meshes.ToArray();
        }

        private static int GetIndiciesResourceIndex(IModelResourceContainer container)
        {
            var header = container.Header;
            var index = 0;

            if(header.PartInfoCount > 0)
            {
                index++;
            }

            if (header.PartInfo2Count > 0)
            {
                index++;
            }

            if (header.PartInfo3Count > 0)
            {
                index++;
            }

            return index;
        }

        private static int GetFirstVertexComponentIndex(IModelResourceContainer container)
        {
            var index = GetIndiciesResourceIndex(container);

            if(container.Header.UknownDataLength > 0)
            {
                index++;
            }

            if(container.Header.UknownIndiciesCount > 0)
            {
                index++;
            }

            return index + 2;
        }

        private static VertexFormat[] ProcessVerticies(IModelResourceContainer container)
        {
            //int firstVertIndex = 0;
            //for(var i = 0; i < container.Resources.Length; i++)
            //{
            //    if(container.Resources[i].Type == ModelResource.ResourceType.VertexAttribute)
            //    {
            //        firstVertIndex = i;
            //        break;
            //    }
            //}

            var vertIndex = GetFirstVertexComponentIndex(container);

            var verts = new VertexFormat[container.VertexCount];

            var posResouce = container.Resources[vertIndex];
            var posData = posResouce.Data.Span;

            for (var i = 0; i < container.VertexCount; i++)
            {
                var vert = new VertexFormat();

                vert.Position = posData.ReadVec3At(i * 12);

                verts[i] = vert;
            }

            var texResouce = container.Resources[vertIndex + 1];
            var texData = texResouce.Data.Span;

            for (var i = 0; i < container.VertexCount; i++)
            {
                var vert = verts[i];

                vert.TexCoords = texData.ReadVec2At(i * 8);

                verts[i] = vert;
            }

            var tbnResouce = container.Resources[vertIndex + 2];
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
