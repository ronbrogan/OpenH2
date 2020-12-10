using OpenH2.Core.Offsets;
using OpenH2.Core.Parsing;
using OpenH2.Core.Tags.Common.Models;
using OpenH2.Core.Tags.Layout;
using OpenBlam.Serialization.Layout;
using OpenH2.Foundation;
using System;
using System.Numerics;
using System.Text.Json.Serialization;
using OpenBlam.Core.MapLoading;
using OpenBlam.Serialization;
using System.Diagnostics;
using System.IO;
using OpenH2.Core.Extensions;

namespace OpenH2.Core.Tags
{
    [TagLabel(TagName.mode)]
    public class RenderModelTag : BaseTag
    { 
        public RenderModelTag(uint id) : base(id)
        {
        }

        [PrimitiveValue(0)]
        public int NameId { get; set; }
        
        [PrimitiveValue(10)]
        public ushort Flags { get; set; }

        [ReferenceArray(20)]
        public BoundingBox[] BoundingBoxes { get; set; }

        [ReferenceArray(28)]
        public Component[] Components { get; set; }

        public DamageLevel[] Permutations { get; set; }

        [ReferenceArray(36)]
        public Part[] Parts { get; set; }

        [ReferenceArray(72)]
        public Bone[] Bones { get; set; }

        [ReferenceArray(88)]
        public NamedSection[] Sections { get; set; }

        [ReferenceArray(96)]
        public ModelShaderReference[] ModelShaderReferences { get; set; }

        public override void PopulateExternalData(MapStream reader)
        {
            
            foreach (var part in Parts)
            {
                var headerOffset = new NormalOffset((int)part.DataBlockRawOffset);
                var mapData = reader.GetStream(headerOffset.Location);
                part.Header = BlamSerializer.Deserialize<ModelResourceBlockHeader>(mapData, headerOffset.Value);

                foreach (var resource in part.Resources)
                {
                    var dataOffset = part.DataBlockRawOffset + 8 + part.DataPreambleSize + resource.Offset;
                    mapData.Position = new NormalOffset((int)dataOffset).Value;

                    var resourceData = new byte[resource.Size];
                    var readCount = mapData.Read(resourceData, 0, resource.Size);

                    Debug.Assert(readCount == resource.Size);

                    resource.Data = resourceData;
                }

                var meshes = ModelResouceContainerProcessor.ProcessContainer(part, ModelShaderReferences);

                if(this.BoundingBoxes.Length > 0)
                {

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

                        // Workaround for JIT issue
                        // https://github.com/dotnet/runtime/issues/1241
                        var newVert = new VertexFormat(newPos,
                            newTex,
                            vert.Normal,
                            vert.Tangent,
                            vert.Bitangent);

                        mesh.Verticies[i] = newVert;
                    }
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
        public class Component
        {
            [InternedString(0)]
            public string PartName { get; set; }

            [ReferenceArray(8)]
            public DamageLevel[] DamageLevels { get; set; }
        }

        [FixedLength(16)]
        public class DamageLevel
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

            [ReferenceArray(72)]
            public ModelResource[] Resources { get; set; }

            [PrimitiveValue(80)]
            public uint ResourceSize { get; set; }

            [PrimitiveValue(84)]
            public uint ResourceOffset { get; set; }

            public MeshCollection Model { get; set; }
        }

        [FixedLength(96)]
        public class Bone
        {
            [InternedString(0)]
            public string Name { get; set; }

            [PrimitiveValue(4)]
            public short ParentIndex { get; set; }

            [PrimitiveValue(6)]
            public short FirstChildIndex { get; set; }

            [PrimitiveValue(8)]
            public short NextSiblingIndex { get; set; }

            [PrimitiveValue(12)]
            public Vector3 Translation { get; set; }

            [PrimitiveValue(24)]
            public Quaternion Orientation { get; set; }

            [PrimitiveValue(36)]
            public Vector2 Unknown1 { get; set; }

            [PrimitiveValue(48)]
            public Vector3 Unknown2 { get; set; }

            [PrimitiveValue(60)]
            public Vector3 Unknown3 { get; set; }

            [PrimitiveValue(72)]
            public Vector3 Unknown4 { get; set; }

            [PrimitiveValue(84)]
            public Vector3 Unknown5 { get; set; }
        }

        [FixedLength(12)]
        public class NamedSection
        {
            [InternedString(0)]
            public string Name { get; set; }

            [ReferenceArray(4)]
            public SectionInfo[] SectionInfos { get; set; }
        }

        [FixedLength(36)]
        public class SectionInfo
        {
            [PrimitiveValue(4)]
            public Vector3 Bounds { get; set; }

            // More floats, stuff
        }
    }
}
