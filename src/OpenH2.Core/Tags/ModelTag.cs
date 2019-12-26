using OpenH2.Core.Offsets;
using OpenH2.Core.Parsing;
using OpenH2.Core.Tags.Common;
using OpenH2.Core.Tags.Layout;
using OpenH2.Foundation;
using System;
using System.Numerics;
using System.Text.Json.Serialization;

namespace OpenH2.Core.Tags
{
    [TagLabel(TagName.mode)]
    public class ModelTag : BaseTag
    { 
        public ModelTag(uint id) : base(id)
        {
        }

        public override string Name { get; set; }

        [PrimitiveValue(0)]
        public int NameId { get; set; }
        
        [PrimitiveValue(10)]
        public ushort Flags { get; set; }

        [InternalReferenceValue(20)]
        public BoundingBox[] BoundingBoxes { get; set; }

        [InternalReferenceValue(28)]
        public LevelOfDetail[] Lods { get; set; }

        public Permutation[] Permutations { get; set; }

        [InternalReferenceValue(36)]
        public Part[] Parts { get; set; }

        // TODO, bones are implict reference from parts...how to support?
        // TODO: bones aren't in tag at all??
        public Bone[] Bones { get; set; }

        [InternalReferenceValue(72)]
        public Marker[] Markers { get; set; }

        [InternalReferenceValue(96)]
        public ModelShaderReference[] ModelShaderReferences { get; set; }

        public override void PopulateExternalData(H2vReader sceneReader)
        {
            foreach (var part in Parts)
            {
                var headerData = sceneReader.Chunk(new NormalOffset((int)part.DataBlockRawOffset), 120, "ModelMeshHeader");

                part.Header = new ModelResourceBlockHeader()
                {
                    PartInfoCount = headerData.ReadUInt32At(8),
                    PartInfo2Count = headerData.ReadUInt32At(16),
                    PartInfo3Count = headerData.ReadUInt32At(24),
                    IndexCount = headerData.ReadUInt32At(40),
                    UknownDataLength = headerData.ReadUInt32At(48),
                    UknownIndiciesCount = headerData.ReadUInt32At(56),
                    VertexComponentCount = headerData.ReadUInt32At(64)
                };

                foreach(var resource in part.Resources)
                {
                    var dataOffset = part.DataBlockRawOffset + 8 + part.DataPreambleSize + resource.Offset;
                    resource.Data = sceneReader.Chunk(new NormalOffset((int)dataOffset), resource.Size, "ModelMesh").ReadArray(0, resource.Size);
                }

                var meshes = ModelResouceContainerProcessor.ProcessContainer(part, ModelShaderReferences);

                var bbIndex = 0;

                var maxBounds = new Vector3(
                    this.BoundingBoxes[bbIndex].MaxX,
                    this.BoundingBoxes[bbIndex].MaxY,
                    this.BoundingBoxes[bbIndex].MaxZ);

                var minBounds = new Vector3(
                    this.BoundingBoxes[bbIndex].MinX,
                    this.BoundingBoxes[bbIndex].MinY,
                    this.BoundingBoxes[bbIndex].MinZ);

                var maxUV = new Vector2(
                    this.BoundingBoxes[bbIndex].MaxU,
                    this.BoundingBoxes[bbIndex].MaxV);

                var minUV = new Vector2(
                    this.BoundingBoxes[bbIndex].MinU,
                    this.BoundingBoxes[bbIndex].MinV);

                var mesh = meshes[0];

                for (var i = 0; i < mesh.Verticies.Length; i++)
                {
                    var vert = mesh.Verticies[i];

                    var newPos = part.Flags.HasFlag(Properties.CompressedVerts) ? new Vector3(
                        Decompress(vert.Position.X, minBounds.X, maxBounds.X),
                        Decompress(vert.Position.Y, minBounds.Y, maxBounds.Y),
                        Decompress(vert.Position.Z, minBounds.Z, maxBounds.Z)
                        ) : vert.Position;

                    var newTex = part.Flags.HasFlag(Properties.CompressedTexCoords) ? new Vector2(
                        Decompress(vert.TexCoords.X, minUV.X, maxUV.X),
                        Decompress(vert.TexCoords.Y, minUV.Y, maxUV.Y)
                        ) : vert.TexCoords;

                    mesh.Verticies[i] = new VertexFormat(newPos,
                        newTex,
                        vert.Normal,
                        vert.Tangent,
                        vert.Bitangent);
                }

                part.Model = new MeshCollection(meshes);
            }
        }

        public float Decompress(float val, float min, float max)
        {
            return (val + 1f) / 2f * (max - min) + min;
        }

        [Flags]
        public enum Properties : short
        {
            CompressedVerts             = 1 << 0,
            CompressedTexCoords         = 1 << 1,
            CompressedSecTexCoords      = 1 << 2,
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

            // RESEARCH: It looks like these are for different damage states, 
            // ie a pillar gets destroyed, different permutations are used
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
            #if DEBUG
            public byte[] RawData { get; set; }
            #endif

            [PrimitiveValue(4)]
            public ushort VertexCount { get; set; }

            [PrimitiveValue(6)]
            public ushort TriangleCount { get; set; }

            [PrimitiveValue(8)]
            public ushort PartCount { get; set; }

            [PrimitiveValue(20)]
            public ushort BoneCount { get; set; }

            [PrimitiveValue(26)]
            public Properties Flags { get; set; }

            [PrimitiveValue(56)]
            public uint DataBlockRawOffset { get; set; }

            [PrimitiveValue(60)]
            public uint DataBlockSize { get; set; }

            [PrimitiveValue(64)]
            public uint DataPreambleSize { get; set; }

            [PrimitiveValue(68)]
            public uint DataBodySize { get; set; }

            public ModelResourceBlockHeader Header { get; set; }

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
