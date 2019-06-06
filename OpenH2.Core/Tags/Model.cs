using OpenH2.Core.Tags.Layout;
using System;

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
