using OpenH2.Core.Enums;
using OpenH2.Core.Extensions;
using OpenH2.Core.Offsets;
using OpenH2.Core.Parsing;
using OpenH2.Core.Tags.Common;
using OpenH2.Core.Tags.Layout;
using OpenH2.Core.Types;
using OpenH2.Foundation;
using System;
using System.Numerics;

namespace OpenH2.Core.Tags
{
    [TagLabel("mode")]
    public class ModelTag : BaseTag
    { 
        public ModelTag(uint id) : base(id)
        {
        }

        public override string Name { get; set; }

        [PrimitiveValue(0)]
        public int NameId { get; set; }

        [InternalReferenceValue(20)]
        public BoundingBox[] BoundingBoxes { get; set; }

        [InternalReferenceValue(28)]
        public LevelOfDetail[] Lods { get; set; }

        
        public Permutation[] Permutations { get; set; }

        [InternalReferenceValue(36)]
        public Part[] Parts { get; set; }

        // TODO, bones are implict reference from parts...how to support?
        public Bone[] Bones { get; set; }

        [InternalReferenceValue(72)]
        public Marker[] Markers { get; set; }

        [InternalReferenceValue(96)]
        public ModelShaderReference[] ModelShaderReferences { get; set; }

        public override void PopulateExternalData(H2vReader sceneReader)
        {
            foreach(var part in Parts)
            {
                foreach(var resource in part.Resources)
                {
                    var dataOffset = part.DataBlockRawOffset + 8 + part.DataPreambleSize + resource.Offset;
                    resource.Data = sceneReader.Chunk(new NormalOffset((int)dataOffset), resource.Size, "ModelMesh").AsMemory();
                }

                var meshes = ModelResouceContainerProcessor.ProcessContainer(part, ModelShaderReferences);
                part.Model = new MeshCollection(meshes);
            }

            //foreach(var part in Parts)
            //{
            //    var offset = new NormalOffset((int)part.DataOffset);

            //    part.Data = sceneReader.Chunk(offset, (int)part.DataSize).AsMemory();
            //    var data = part.Data.Span;

            //    var mesh = new ModeMesh();

            //    mesh.ShaderCount = data.ReadUInt32At(8);
            //    mesh.UnknownCount = data.ReadUInt32At(16);
            //    mesh.IndiciesCount = data.ReadUInt32At(40);
            //    mesh.BoneCount = data.ReadUInt32At(108);

            //    mesh.ShaderData = new Memory<byte>[mesh.ShaderCount];
            //    for (var i = 0; i < mesh.ShaderCount; i++)
            //    {
            //        mesh.ShaderData[i] = new Memory<byte>(
            //            data.Slice(mesh.ShaderDataOffset + (i * mesh.ShaderChunkSize), 4 + (int)(mesh.ShaderCount * mesh.ShaderChunkSize))
            //            .ToArray());
            //    }

            //    mesh.UnknownData = new Memory<byte>[mesh.UnknownCount];
            //    for (var i = 0; i < mesh.UnknownCount; i++)
            //    {
            //        mesh.UnknownData[i] = new Memory<byte>(
            //            data.Slice(mesh.UnknownDataOffset + (i * mesh.UnknownChunkSize), 4 + (int)(mesh.UnknownCount * mesh.UnknownChunkSize))
            //            .ToArray());
            //    }

            //    mesh.Indicies = new int[mesh.IndiciesCount];
            //    for (var i = 0; i < mesh.IndiciesCount; i++)
            //    {
            //        mesh.Indicies[i] = data.ReadUInt16At(mesh.IndiciesDataOffset + 4 + (2 * i));
            //    }

            //    mesh.Verticies = new VertexFormat[part.VertexCount];
            //    for (var i = 0; i < part.VertexCount; i++)
            //    {
            //        var basis = mesh.VertexDataOffset + 4 + (i * 12);

            //        var vert = new VertexFormat();
            //        vert.Position = new Vector3(
            //            data.ReadFloatAt(basis),
            //            data.ReadFloatAt(basis + 4),
            //            data.ReadFloatAt(basis + 8));

            //        mesh.Verticies[i] = vert;
            //    }

            //    part.Mesh = mesh;
            //}


        }

        [FixedLength(56)]
        public class BoundingBox
        {
            [PrimitiveValue(0)]
            public float MinX { get; set; }

            [PrimitiveValue(4)]
            public float MaxX { get; set; }

            [PrimitiveValue(8)]
            public float MinY { get; set; }

            [PrimitiveValue(12)]
            public float MaxY { get; set; }

            [PrimitiveValue(16)]
            public float MinZ { get; set; }

            [PrimitiveValue(20)]
            public float MaxZ { get; set; }

            [PrimitiveValue(24)]
            public float MinU { get; set; }

            [PrimitiveValue(28)]
            public float MaxU { get; set; }

            [PrimitiveValue(32)]
            public float MinV { get; set; }

            [PrimitiveValue(36)]
            public float MaxV { get; set; }
        }

        [FixedLength(16)]
        public class LevelOfDetail
        {
            [PrimitiveValue(0)]
            public int PartNameId { get; set; }

            [InternalReferenceValue(8)]
            public Permutation[] Permutations { get; set; }
        }

        [FixedLength(16)]
        public class Permutation
        {
            [PrimitiveValue(0)]
            public int PermutationNameId { get; set; }

            [PrimitiveValue(4)]
            public short LowestPieceIndex { get; set; }

            [PrimitiveValue(6)]
            public short LowPieceIndex { get; set; }

            [PrimitiveValue(8)]
            public short MediumLowPieceIndex { get; set; }

            [PrimitiveValue(10)]
            public short MediumHighPieceIndex { get; set; }

            [PrimitiveValue(12)]
            public short HighPieceIndex { get; set; }

            [PrimitiveValue(14)]
            public short HighestPieceIndex { get; set; }
        }

        [FixedLength(92)]
        public class Part : IModelResourceContainer
        {
            [PrimitiveValue(0)]
            public uint Type { get; set; } // TODO: make an enum with types

            [PrimitiveValue(4)]
            public ushort VertexCount { get; set; }

            [PrimitiveValue(6)]
            public ushort TriangleCount { get; set; }

            [PrimitiveValue(20)]
            public ushort BoneCount { get; set; }

            [PrimitiveValue(56)]
            public uint DataBlockRawOffset { get; set; }

            [PrimitiveValue(60)]
            public uint DataBlockSize { get; set; }

            [PrimitiveValue(64)]
            public uint DataPreambleSize { get; set; }

            [PrimitiveValue(68)]
            public uint DataBodySize { get; set; }

            public Memory<byte> Data { get; set; }

            [InternalReferenceValue(72)]
            public ModelResource[] Resources { get; set; }

            [PrimitiveValue(80)]
            public uint ResourceSize { get; set; }

            [PrimitiveValue(84)]
            public uint ResourceOffset { get; set; }

            public MeshCollection Model { get; set; }
        }

        // TODO, I think bones are just always after parts, without explicit reference...
        // Serializer won't support that...
        [FixedLength(128)]
        public class Bone
        {
            [PrimitiveValue(0)]
            public int BoneNameId { get; set; }

            [PrimitiveValue(4)]
            public short ParentIndex { get; set; }

            [PrimitiveValue(6)]
            public short FirstChildIndex { get; set; }

            [PrimitiveValue(8)]
            public short NextSiblingIndex { get; set; }

            [PrimitiveValue(12)]
            public float X { get; set; }

            [PrimitiveValue(16)]
            public float Y { get; set; }

            [PrimitiveValue(20)]
            public float Z { get; set; }

            [PrimitiveValue(24)]
            public float Yaw { get; set; }

            [PrimitiveValue(28)]
            public float Pitch { get; set; }

            [PrimitiveValue(32)]
            public float Roll { get; set; }

            [PrimitiveValue(70)]
            public float SingleBoneX { get; set; }

            [PrimitiveValue(74)]
            public float SingleBoneY { get; set; }

            [PrimitiveValue(78)]
            public float SingleBoneZ { get; set; }

            [PrimitiveValue(82)]
            public float DistanceFromParent { get; set; }
        }

        [FixedLength(96)]
        public class Marker
        {
            [PrimitiveValue(0)]
            public int MarkerNameId { get; set; }

            [PrimitiveValue(36)]
            public float X { get; set; }

            [PrimitiveValue(40)]
            public float Y { get; set; }

            [PrimitiveValue(44)]
            public float Z { get; set; }

            [PrimitiveValue(48)]
            public float Yaw { get; set; }

            [PrimitiveValue(52)]
            public float Pitch { get; set; }

            [PrimitiveValue(56)]
            public float Roll { get; set; }
        }
    }
}
