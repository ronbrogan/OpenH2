using OpenH2.Core.Extensions;
using OpenH2.Core.Meta;
using OpenH2.Core.Parsing;
using OpenH2.Core.Types;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace OpenH2.Core.Tags.Processors
{
    public static class ModelTagProcessor
    {
        public static ModelTagNode ProcessModelMeta(BaseMeta meta, TrackingReader reader)
        {
            var modelMeta = (ModelMeta)meta;
            var node = new ModelTagNode();

            node.Meta = modelMeta;
            node.RawPartData = new Memory<byte>[modelMeta.PartCount];
            node.Parts = new Mesh[modelMeta.PartCount];

            for (var i = 0; i < modelMeta.PartCount; i++)
            {
                var part = modelMeta.Parts[i];
                var span = reader.Chunk((int)part.DataOffset, (int)part.DataSize, "Model").Span;

                node.RawPartData[i] = span.ToArray();

                node.Parts[i] = GetMeshFromPart(part, span);
            }

            return node;
        }

        private static Mesh GetMeshFromPart(ModelMeta.Part part, Span<byte> data)
        {
            var mesh = new Mesh();

            mesh.ShaderCount = data.ReadUInt32At(8);
            mesh.UnknownCount = data.ReadUInt32At(16);
            mesh.IndiciesCount = data.ReadUInt32At(40);
            mesh.BoneCount = data.ReadUInt32At(108);

            mesh.ShaderData = new Memory<byte>[mesh.ShaderCount];
            for(var i = 0; i < mesh.ShaderCount; i++)
            {
                mesh.ShaderData[i] = new Memory<byte>(
                    data.Slice(mesh.ShaderDataOffset + (i * mesh.ShaderChunkSize), 4 + (int)(mesh.ShaderCount * mesh.ShaderChunkSize))
                    .ToArray());
            }

            mesh.UnknownData = new Memory<byte>[mesh.UnknownCount];
            for (var i = 0; i < mesh.UnknownCount; i++)
            {
                mesh.UnknownData[i] = new Memory<byte>(
                    data.Slice(mesh.UnknownDataOffset + (i * mesh.UnknownChunkSize), 4 + (int)(mesh.UnknownCount * mesh.UnknownChunkSize))
                    .ToArray());
            }

            mesh.Indicies = new ushort[mesh.IndiciesCount];
            for (var i = 0; i < mesh.IndiciesCount; i++)
            {
                mesh.Indicies[i] = data.ReadUInt16At(mesh.IndiciesDataOffset + 4 + (2 * i));
            }

            mesh.Verticies = new Vertex[part.VertexCount];
            for (var i = 0; i < part.VertexCount; i++)
            {
                var basis = mesh.VertexDataOffset + 4 + (i * 12);

                var vert = new Vertex();
                vert.Position = new Vector3(
                    data.ReadFloatAt(basis),
                    data.ReadFloatAt(basis + 4),
                    data.ReadFloatAt(basis + 8));

                mesh.Verticies[i] = vert;
            }

            return mesh;
        }
    }
}
