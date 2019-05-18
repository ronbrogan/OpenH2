using OpenH2.Core.Extensions;
using OpenH2.Core.Tags;
using OpenH2.Core.Types;
using System;
using System.Numerics;

namespace OpenH2.Translation.TagData.Processors
{
    public static class ModelTagDataProcessor
    {
        public static ModelTagData ProcessTag(BaseTag tag)
        {
            var model = tag as Model;

            if (model == null)
                throw new ArgumentException("Tag must be a Model", nameof(tag));

            var tagData = new ModelTagData(model)
            {
                Parts = new Mesh[model.PartCount],
                Name = model.Name
            };

            for (var i = 0; i < model.PartCount; i++)
            {
                var part = model.Parts[i];

                tagData.Parts[i] = GetMeshFromPart(part);
            }

            return tagData;
        }

        private static Mesh GetMeshFromPart(Model.Part part)
        {
            var mesh = new Mesh();

            var data = part.Data.Span;

            // HACK: Once we get data from shared files, this can be removed
            if(data.ReadStringFrom(0, 5) == "ERROR")
            {
                return mesh;
            }

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
