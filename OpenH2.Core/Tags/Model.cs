using OpenH2.Core.Extensions;
using OpenH2.Core.Parsing;
using OpenH2.Core.Tags.Layout;
using OpenH2.Core.Types;
using System;
using System.Numerics;

namespace OpenH2.Core.Tags
{
    [TagLabel("mode")]
    public class Model : BaseTag
    { 
        public Model(uint id) : base(id)
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
        public Shader[] Shaders { get; set; }

        public override void PopulateExternalData(TrackingReader sceneReader)
        {
            foreach(var part in Parts)
            {
                part.Data = sceneReader.Chunk((int)part.DataOffset, (int)part.DataSize).AsMemory();
                var data = part.Data.Span;

                var mesh = new ModeMesh();

                // HACK: Once we get data from shared files, this can be removed
                //if (data.ReadStringFrom(0, 5) == "ERROR")
                //{
                //    return mesh;
                //}

                mesh.ShaderCount = data.ReadUInt32At(8);
                mesh.UnknownCount = data.ReadUInt32At(16);
                mesh.IndiciesCount = data.ReadUInt32At(40);
                mesh.BoneCount = data.ReadUInt32At(108);

                mesh.ShaderData = new Memory<byte>[mesh.ShaderCount];
                for (var i = 0; i < mesh.ShaderCount; i++)
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

                part.Mesh = mesh;
            }


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
        public class Part
        {
            [PrimitiveValue(0)]
            public uint Type { get; set; } // TODO: make an enum with types

            [PrimitiveValue(4)]
            public ushort VertexCount { get; set; }

            [PrimitiveValue(20)]
            public ushort BoneCount { get; set; }

            [PrimitiveValue(56)]
            public uint DataOffset { get; set; }

            [PrimitiveValue(60)]
            public uint DataSize { get; set; }

            public Memory<byte> Data { get; set; }

            [PrimitiveValue(64)]
            public uint DataHeaderSize { get; set; }

            [PrimitiveValue(68)]
            public uint DataBodySize { get; set; }

            [PrimitiveValue(80)]
            public uint ResourceSize { get; set; }

            [PrimitiveValue(84)]
            public uint ResouceOffset { get; set; }

            public ModeMesh Mesh { get; set; }
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

        [FixedLength(32)]
        public class Shader
        {
            [PrimitiveValue(12)]
            public uint ShaderId { get; set; }
        }
    }
}
