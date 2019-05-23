using System;
using OpenH2.Core.Offsets;
using OpenH2.Core.Tags.Layout;

namespace OpenH2.Core.Tags
{
    public class Model : BaseTag
    {
        public Model(uint id) : base(id)
        {
        }

        public override string Name { get; set; }

        [PrimitiveValue(0)]
        public int NameId { get; set; }

        [PrimitiveValue(0)]
        public int BoundingBoxCount { get; set; }
        public TagInternalOffset BoundingBoxesOffset { get; set; }
        public int LodCount { get; set; }
        public TagInternalOffset LodsOffset { get; set; }
        public int PartCount { get; set; }
        public TagInternalOffset PartsOffset { get; set; }
        public int MarkerCount { get; set; }
        public TagInternalOffset MarkersOffset { get; set; }
        public int ShaderCount { get; set; }
        public TagInternalOffset ShadersOffset { get; set; }



        [InternalReferenceValue(20)]
        public BoundingBox[] BoundingBoxes { get; set; }

        //[InternalReferenceValue(28)]
        public LevelOfDetail[] Lods { get; set; }

        
        public Permutation[] Permutations { get; set; }

        //[InternalReferenceValue(36)]
        public Part[] Parts { get; set; }
        public Bone[] Bones { get; set; }

        //[InternalReferenceValue(72)]
        public Marker[] Markers { get; set; }

        //[InternalReferenceValue(96)]
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

            public static int Length => 56;
        }

        public class LevelOfDetail
        {
            public int PartNameId { get; set; }
            public int PermutationCount { get; set; }
            public TagInternalOffset PermutationsOffset { get; set; }

            public Permutation[] Permutations { get; set; }

            public static int Length => 16;
        }

        public class Permutation
        {
            public int PermutationNameId { get; set; }
            public short LowestPieceIndex { get; set; }
            public short LowPieceIndex { get; set; }
            public short MediumLowPieceIndex { get; set; }
            public short MediumHighPieceIndex { get; set; }
            public short HighPieceIndex { get; set; }
            public short HighestPieceIndex { get; set; }

            public static int Length => 16;
        }

        public class Part
        {
            public uint Type { get; set; } // TODO: make an enum with types
            public ushort VertexCount { get; set; }
            public ushort BoneCount { get; set; }
            public uint DataOffset { get; set; }
            public uint DataSize { get; set; }
            public Memory<byte> Data { get; set; }
            public uint DataHeaderSize { get; set; }
            public uint DataBodySize { get; set; }
            public uint ResourceSize { get; set; }
            public uint ResouceOffset { get; set; }

            public static int Length => 92;
        }

        public class Bone
        {
            public int BoneNameId { get; set; }
            public short ParentIndex { get; set; }
            public short FirstChildIndex { get; set; }
            public short NextSiblingIndex { get; set; }
            public float X { get; set; }
            public float Y { get; set; }
            public float Z { get; set; }
            public float Yaw { get; set; }
            public float Pitch { get; set; }
            public float Roll { get; set; }
            public float SingleBoneX { get; set; }
            public float SingleBoneY { get; set; }
            public float SingleBoneZ { get; set; }
            public float DistanceFromParent { get; set; }

            public static int Length => 128;
        }

        public class Marker
        {
            public int MarkerNameId { get; set; }
            public float X { get; set; }
            public float Y { get; set; }
            public float Z { get; set; }
            public float Yaw { get; set; }
            public float Pitch { get; set; }
            public float Roll { get; set; }

            public static int Length => 96;
        }

        public class Shader
        { 
            public uint ShaderId { get; set; }

            public static int Length => 32;
        }
    }
}
