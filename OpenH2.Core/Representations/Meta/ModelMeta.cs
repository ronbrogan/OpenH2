using OpenH2.Core.Offsets;
using System.Runtime.InteropServices;

namespace OpenH2.Core.Representations.Meta
{
    public class ModelMeta : BaseMeta
    {
        public override string Name { get; set; }

        public int NameId { get; set; }
        public int BoundingBoxCount { get; set; }
        public MetaOffset BoundingBoxesOffset { get; set; }
        public int LodCount { get; set; }
        public MetaOffset LodsOffset { get; set; }
        public int PartCount { get; set; }
        public MetaOffset PartsOffset { get; set; }
        public int MarkerCount { get; set; }
        public MetaOffset MarkersOffset { get; set; }
        public int ShaderCount { get; set; }
        public MetaOffset ShadersOffset { get; set; }




        public BoundingBox[] BoundingBoxes { get; set; }
        public LevelOfDetail[] Lods { get; set; }
        public Permutation[] Permutations { get; set; }
        public Part[] Parts { get; set; }
        public Bone[] Bones { get; set; }
        public Marker[] Markers { get; set; }
        public Shader[] Shaders { get; set; }

        public class BoundingBox
        {
            public float MinX { get; set; }
            public float MaxX { get; set; }
            public float MinY { get; set; }
            public float MaxY { get; set; }
            public float MinZ { get; set; }
            public float MaxZ { get; set; }
            public float MinU { get; set; }
            public float MaxU { get; set; }
            public float MinV { get; set; }
            public float MaxV { get; set; }

            public static int Length => 56;
        }

        public class LevelOfDetail
        {
            public int PartNameId { get; set; }
            public int Unknown { get; set; } // In Observed example, this unknown is always 1
            public int OffsetToPermutations { get; set; }

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
