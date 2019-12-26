using OpenH2.Core.Extensions;
using OpenH2.Core.Representations;
using OpenH2.Foundation;
using System;
using System.Collections.Generic;

namespace OpenH2.Core.Tags.Common
{
    public static class ModelResouceContainerProcessor
    {
        private struct PartDescription
        {
            public PartDescription(int indexStart, int indexCount, TagRef<ShaderTag> shader, MeshElementType elementType)
            {
                IndexStart = indexStart;
                IndexCount = indexCount;
                Shader = shader;
                ElementType = elementType;
            }

            public int IndexStart;
            public int IndexCount;
            public TagRef<ShaderTag> Shader;
            public MeshElementType ElementType;
        }

        public static ModelMesh[] ProcessContainer(IModelResourceContainer container, ModelShaderReference[] shaders, string note = null)
        {
            var parts = new List<PartDescription>((int)container.Header.PartInfoCount);

            var verts = new VertexFormat[container.VertexCount];
            Span<int> indices = new int[container.Header.IndexCount];

            var currentResource = 0;

            // process part info 0 resource
            if(container.Header.PartInfoCount > 0)
            {
                var partData = container.Resources[currentResource].Data.Span;

                for (var i = 0; i < container.Header.PartInfoCount; i++)
                {
                    var start = i * 72;

                    var elementType = (MeshElementType)partData.ReadUInt16At(start + 2);
                    var matId = partData.ReadUInt16At(start + 4);
                    var indexStart = partData.ReadUInt16At(start + 6);
                    var indexCount = partData.ReadUInt16At(start + 8);

                    var partDescription = new PartDescription(indexStart, indexCount, shaders[matId].ShaderId, elementType);

                    parts.Add(partDescription);
                }

                currentResource++;
            }

            // process part info 2 resource
            if(container.Header.PartInfo2Count > 0)
            {
                // Not positive on what this is for, last ushort of the 8 bytes looks to be part index
                currentResource++;
            }

            // process part info 3 resource
            if (container.Header.PartInfo3Count > 0)
            {
                currentResource++;
            }

            // process indicies resourc
            if(container.Header.IndexCount > 0)
            {
                var data = container.Resources[currentResource].Data.Span;

                for (var i = 0; i < container.Header.IndexCount; i++)
                    indices[i] = data.ReadUInt16At(i * 2);
                
                currentResource++;
            }

            // process unknown resource
            if (container.Header.UknownDataLength > 0)
            {
                currentResource++;
            }

            // process unknown resource
            if (container.Header.UknownIndiciesCount > 0)
            {
                currentResource++;
            }

            // process Vertex Attribute Hint resource
            if (container.Header.VertexComponentCount > 0)
            {
                currentResource++;
            }

            // process vertex attribute resources
            if (container.Header.VertexComponentCount > 0)
            {
                if(container.Header.VertexComponentCount >= 1)
                {
                    var posData = container.Resources[currentResource].Data.Span;

                    for (var i = 0; i < container.VertexCount; i++)
                    {
                        var vert = new VertexFormat();

                        vert.Position = posData.ReadVec3At(i * 12);

                        verts[i] = vert;
                    }

                    currentResource++;
                }

                if (container.Header.VertexComponentCount >= 2)
                {
                    var texData = container.Resources[currentResource].Data.Span;

                    for (var i = 0; i < container.VertexCount; i++)
                    {
                        var vert = verts[i];

                        vert.TexCoords = texData.ReadVec2At(i * 8);

                        verts[i] = vert;
                    }

                    currentResource++;
                }

                if (container.Header.VertexComponentCount >= 3)
                {
                    var tbnData = container.Resources[currentResource].Data.Span;

                    for (var i = 0; i < container.VertexCount; i++)
                    {
                        var vert = verts[i];

                        var start = i * 36;
                        vert.Normal = tbnData.ReadVec3At(start);
                        vert.Bitangent = tbnData.ReadVec3At(start + 12);
                        vert.Tangent = tbnData.ReadVec3At(start + 24);

                        verts[i] = vert;
                    }

                    currentResource++;
                }
            }

            var meshes = new List<ModelMesh>(parts.Count);

            foreach(var part in parts)
            {
                meshes.Add(new ModelMesh
                {
                    Verticies = verts,
                    Indices = indices.Slice(part.IndexStart, part.IndexCount).ToArray(),
                    Shader = part.Shader,
                    ElementType = part.ElementType,
                    Note = note
                });
            }

            return meshes.ToArray();
        }        
    }
}
