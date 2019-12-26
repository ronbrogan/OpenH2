using OpenH2.Core.Offsets;
using OpenH2.Core.Parsing;
using OpenH2.Core.Representations;
using OpenH2.Core.Tags.Common;
using OpenH2.Core.Tags.Layout;
using System;
using System.Numerics;
using System.Text.Json.Serialization;

namespace OpenH2.Core.Tags
{
    [TagLabel(TagName.sbsp)]
    public class BspTag : BaseTag
    {
        [JsonIgnore]
        public byte[] RawMeta { get; set; }

        public override string Name { get; set; }

        public BspTag(uint id) : base(id)
        {
        }

        [PrimitiveValue(8)]
        public int Checksum { get; set; }

        [InternalReferenceValue(12)]
        public ShaderInfo[] Shaders { get; set; }

        [InternalReferenceValue(20)]
        [JsonIgnore]
        public CollisionInfo[] CollisionInfos { get; set; }


        //[InternalReferenceValue(76)]
        //public object[] MiscObject1Cao { get; set; }

        //[InternalReferenceValue(84)]
        //public object[] MiscObject2Cao { get; set; }

        [InternalReferenceValue(92)] 
        public Obj92[] Obj92s { get; set; }

        //[InternalReferenceValue(100)]
        //public Obj100[] Obj100s { get; set; } 

        //[InternalReferenceValue(148)]
        //public object[] MiscObject5Cao { get; set; } 

        [InternalReferenceValue(156)]
        public RenderChunk[] RenderChunks { get; set; }

        [InternalReferenceValue(164)]
        public ModelShaderReference[] ModelShaderReferences { get; set; }

        //[InternalReferenceValue(172)]
        //public object[] MiscObject8Cao { get; set; } 

        //[InternalReferenceValue(212)]
        //public object[] MiscObject9Cao { get; set; } 

        //[InternalReferenceValue(220)]
        //public object[] MiscObject10Cao { get; set; }

        [InternalReferenceValue(244)]
        public DecalInstance[] DecalInstances { get; set; }


        //[InternalReferenceValue(252)]
        //public object[] MiscObject11Cao { get; set; }

        //[InternalReferenceValue(260)]
        //public object[] MiscObject12Cao { get; set; }

        [InternalReferenceValue(312)]
        public InstancedGeometryDefinition[] InstancedGeometryDefinitions { get; set; }

        [InternalReferenceValue(320)] 
        public InstancedGeometryInstance[] InstancedGeometryInstances { get; set; }

        //[InternalReferenceValue(328)]
        //public object[] MiscObject15Cao { get; set; }

        //[InternalReferenceValue(336)]
        //public object[] MiscObject16Cao { get; set; }

        [InternalReferenceValue(344)]
        public Obj344[] Obj344s { get; set; }

        //[InternalReferenceValue(464)]
        //public object[] MiscObject18Cao { get; set; }

        //[InternalReferenceValue(480)]
        //public object[] MiscObject19Cao { get; set; }

        [InternalReferenceValue(524)] 
        public Obj524[] Obj524s { get; set; }


        //[InternalReferenceValue(540)]
        //public object[] MiscObject20Cao { get; set; }

        //[InternalReferenceValue(548)]
        //public object[] MiscObject21Cao { get; set; }

        //[InternalReferenceValue(556)]
        //public object[] MiscObject22Cao { get; set; }

        //[InternalReferenceValue(564)]
        //public object[] MiscObject23Cao { get; set; }


        public override void PopulateExternalData(H2vReader sceneReader)
        {
            foreach (var part in RenderChunks)
            {
                if (part.DataBlockRawOffset == uint.MaxValue)
                {
                    Console.WriteLine("Bsp part with max DataBlock offset");
                    part.Model = new MeshCollection(new ModelMesh[0]);
                    continue;
                }

                var headerData = sceneReader.Chunk(new NormalOffset((int)part.DataBlockRawOffset), (int)part.DataPreambleSize, "ModelMeshHeader");

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

                foreach (var resource in part.Resources)
                {
                    var dataOffset = part.DataBlockRawOffset + 8 + part.DataPreambleSize + resource.Offset;
                    resource.Data = sceneReader.Chunk(new NormalOffset((int)dataOffset), resource.Size, "Bsp Render Data").ReadArray(0, resource.Size);
                }

                var meshes = ModelResouceContainerProcessor.ProcessContainer(part, ModelShaderReferences);
                part.Model = new MeshCollection(meshes);
            }

            foreach (var def in InstancedGeometryDefinitions)
            {
                if (def.DataBlockRawOffset == uint.MaxValue)
                {
                    Console.WriteLine("InstancedGeometry with max DataBlock offset");
                    def.Model = new MeshCollection(new ModelMesh[0]);
                    continue;
                }

                var headerData = sceneReader.Chunk(new NormalOffset((int)def.DataBlockRawOffset), (int)def.DataPreambleSize, "InstancedGeometryMeshHeader");

                def.Header = new ModelResourceBlockHeader()
                {
                    PartInfoCount = headerData.ReadUInt32At(8),
                    PartInfo2Count = headerData.ReadUInt32At(16),
                    PartInfo3Count = headerData.ReadUInt32At(24),
                    IndexCount = headerData.ReadUInt32At(40),
                    UknownDataLength = headerData.ReadUInt32At(48),
                    UknownIndiciesCount = headerData.ReadUInt32At(56),
                    VertexComponentCount = headerData.ReadUInt32At(64)
                };

                foreach (var resource in def.Resources)
                {
                    var dataOffset = def.DataBlockRawOffset + 8 + def.DataPreambleSize + resource.Offset;
                    resource.Data = sceneReader.Chunk(new NormalOffset((int)dataOffset), resource.Size, "InstancedGeometry Render Data").ReadArray(0, resource.Size);
                }

                var meshes = ModelResouceContainerProcessor.ProcessContainer(def, ModelShaderReferences, "InstancedGeometry_" + def.DataBlockRawOffset);
                def.Model = new MeshCollection(meshes);
            }
        }

        [FixedLength(20)]
        public class ShaderInfo
        {
            [PrimitiveValue(4)]
            public int Unknown { get; set; }

            [PrimitiveValue(8)]
            public ushort Value1 { get; set; }

            [PrimitiveValue(16)]
            public TagRef<ShaderTag> Shader { get; set; }
        }

        [FixedLength(68)]
        public class CollisionInfo
        {
            [InternalReferenceValue(0)]
            public RawObject1[] RawObject1s { get; set; }

            [InternalReferenceValue(8)]
            public RawObject2[] RawObject2s { get; set; }

            [InternalReferenceValue(16)]
            public RawObject3[] RawObject3s { get; set; }

            [InternalReferenceValue(24)]
            public RawObject4[] RawObject4s { get; set; }

            [InternalReferenceValue(32)]
            public RawObject5[] RawObject5s { get; set; }

            [InternalReferenceValue(40)]
            public Face[] Faces { get; set; }

            [InternalReferenceValue(48)]
            public HalfEdgeContainer[] HalfEdges { get; set; }

            [InternalReferenceValue(56)]
            public Vertex[] Verticies { get; set; }

            [PrimitiveValue(64)]
            public int Unknown { get; set; }

            [FixedLength(8)]
            public class RawObject1
            {
                [PrimitiveValue(0)]
                public ushort val1 { get; set; }

                [PrimitiveValue(2)]
                public ushort val2 { get; set; }

                [PrimitiveValue(4)]
                public ushort unknown1 { get; set; }

                [PrimitiveValue(6)]
                public ushort unknown2 { get; set; }
            }

            [FixedLength(16)]
            public class RawObject2
            {
                [PrimitiveValue(0)]
                public float x { get; set; }

                [PrimitiveValue(4)]
                public float y { get; set; }

                [PrimitiveValue(8)]
                public float z { get; set; }

                [PrimitiveValue(12)]
                public float w { get; set; }
            }

            [FixedLength(4)]
            public class RawObject3
            {
                [PrimitiveValue(0)]
                public ushort val1 { get; set; }

                [PrimitiveValue(2)]
                public ushort val2 { get; set; }
            }

            [FixedLength(4)]
            public class RawObject4
            {
                [PrimitiveValue(0)]
                public ushort val1 { get; set; }

                [PrimitiveValue(2)]
                public ushort val2 { get; set; }
            }

            [FixedLength(16)]
            public class RawObject5
            {
                [PrimitiveValue(0)]
                public float x { get; set; }

                [PrimitiveValue(4)]
                public float y { get; set; }

                [PrimitiveValue(8)]
                public float z { get; set; }

                [PrimitiveValue(12)]
                public short u { get; set; }

                [PrimitiveValue(12)]
                public short v { get; set; }
            }

            [FixedLength(8)]
            public class Face
            {
                [PrimitiveValue(0)]
                public ushort val1 { get; set; }

                [PrimitiveValue(2)]
                public ushort FirstEdge { get; set; }

                [PrimitiveValue(4)]
                public ushort val3 { get; set; }

                [PrimitiveValue(6)]
                public ushort val4 { get; set; }
            }

            [FixedLength(12)]
            public class HalfEdgeContainer
            {
                [PrimitiveValue(0)]
                public ushort Vertex0 { get; set; }

                [PrimitiveValue(2)]
                public ushort Vertex1 { get; set; }

                [PrimitiveValue(4)]
                public ushort NextEdge { get; set; }

                [PrimitiveValue(6)]
                public ushort PrevEdge { get; set; }

                [PrimitiveValue(8)]
                public ushort Face0 { get; set; }

                [PrimitiveValue(10)]
                public ushort Face1 { get; set; }
            }

            [FixedLength(16)]
            public class Vertex
            {
                [PrimitiveValue(0)]
                public float x { get; set; }

                [PrimitiveValue(4)]
                public float y { get; set; }

                [PrimitiveValue(8)]
                public float z { get; set; }

                [PrimitiveValue(12)]
                public int edge { get; set; }
            }
        }

        [FixedLength(36)]
        public class Obj92
        {
            [PrimitiveValue(8)]
            public Vector3 Position { get; set; }

            [PrimitiveValue(20)]
            public float Yaw { get; set; }
        }

        [FixedLength(24)]
        public class Obj100 { }

        [FixedLength(176)]
        public class RenderChunk : IModelResourceContainer
        {
            [PrimitiveValue(0)]
            public ushort VertexCount { get; set; }

            [PrimitiveValue(2)]
            public ushort TriangleCount { get; set; }

            public ushort IndiciesIndex { get; set; } = 2;

            [PrimitiveValue(40)]
            public uint DataBlockRawOffset { get; set; }

            [PrimitiveValue(44)]
            public uint DataBlockSize { get; set; }

            [PrimitiveValue(48)]
            public uint DataPreambleSize { get; set; }

            [PrimitiveValue(52)]
            public uint ResourceSubsectionSize { get; set; }

            [InternalReferenceValue(56)]
            public ModelResource[] Resources { get; set; }

            public ModelResourceBlockHeader Header { get; set; }

            public MeshCollection Model { get; set; }
        }

        [FixedLength(16)] 
        public class DecalInstance
        {
            [PrimitiveValue(0)]
            public Vector3 Position { get; set; }

            [PrimitiveValue(12)]
            public ushort Index { get; set; }

            [PrimitiveValue(14)]
            public ushort Unknown { get; set; }
        }

        [FixedLength(200)]
        public class InstancedGeometryDefinition : IModelResourceContainer
        {
            [PrimitiveValue(0)]
            public ushort VertexCount { get; set; }

            [PrimitiveValue(2)]
            public ushort TriangleCount { get; set; }

            [PrimitiveValue(12)]
            public ushort IndiciesIndex { get; set; }

            [InternalReferenceValue(24)]
            public Obj24[] Obj24s { get; set; }

            [PrimitiveValue(40)]
            public uint DataBlockRawOffset { get; set; }

            [PrimitiveValue(44)]
            public uint DataBlockSize { get; set; }

            [PrimitiveValue(48)]
            public uint DataPreambleSize { get; set; }

            [PrimitiveValue(52)]
            public uint DataBodySize { get; set; }

            public ModelResourceBlockHeader Header { get; set; }

            [InternalReferenceValue(56)]
            public ModelResource[] Resources { get; set; }

            public MeshCollection Model { get; set; }

            // I think many of these objects are a half-edge structure for 
            // the individual object like is on the BSP
            [FixedLength(56)]
            public class Obj24
            {
                [PrimitiveArray(0, 10)]
                public float[] Floats { get; set; }
            }


            [FixedLength(8)]
            public class Obj112
            {
                [PrimitiveArray(0,4)]
                public short Shorts { get; set; }
            }

            [FixedLength(16)]
            public class Obj120
            {
                [PrimitiveValue(0)]
                public Vector3 Position { get; set; }

                [PrimitiveValue(12)]
                public float Rotation { get; set; }
            }

            [FixedLength(4)]
            public class Obj128
            {
                [PrimitiveValue(0)]
                public ushort Flags { get; set; }

                [PrimitiveValue(2)]
                public ushort Index { get; set; }
            }

            [FixedLength(4)]
            public class Obj136
            {
                [PrimitiveValue(0)]
                public ushort Index { get; set; }

                [PrimitiveValue(2)]
                public byte Unknown { get; set; }


                [PrimitiveValue(3)]
                public byte Flags { get; set; }
            }

            [FixedLength(16)]
            public class Obj144
            {
                [PrimitiveValue(0)]
                public Vector3 Position { get; set; }

                [PrimitiveValue(12)]
                public ushort Start { get; set; }

                [PrimitiveValue(14)]
                public ushort End { get; set; }
            }

            [FixedLength(8)]
            public class Obj152
            {
                [PrimitiveValue(0)]
                public ushort Start { get; set; }

                [PrimitiveValue(2)]
                public ushort Count { get; set; }

                [PrimitiveValue(4)]
                public uint UnusedMabye{ get; set; }
            }

            [FixedLength(12)]
            public class Obj160
            {

            }

            [FixedLength(16)]
            public class Obj168
            {

            }

            // Likely the Havok collision info
            [FixedLength(112)]
            public class Obj176
            {

            }
            [FixedLength(8)]
            public class Obj184
            {

            }
            [FixedLength(8)]
            public class Obj192
            {

            }
        }

        [FixedLength(88)]
        public class InstancedGeometryInstance
        {
            [PrimitiveValue(0)]
            public float Scale { get; set; }

            [PrimitiveArray(4, 9)]
            public float[] RotationMatrix { get; set; }

            [PrimitiveValue(40)]
            public Vector3 Position { get; set; }

            [PrimitiveValue(52)]
            public uint Index { get; set; }

            [PrimitiveValue(82)]
            public ushort Flags { get; set; }
        }

        [FixedLength(20)]
        public class Obj344
        {
            [PrimitiveValue(0)]
            public ushort Index { get; set; }

            [PrimitiveValue(2)]
            public ushort Unknown { get; set; }

            [PrimitiveArray(4, 4)]
            public float[] Values { get; set; }
        }

        [FixedLength(32)] 
        public class Obj524 
        {
            [PrimitiveValue(0)]
            public ushort Max { get; set; }

            [PrimitiveValue(2)]
            public ushort Index { get; set; }

            [PrimitiveValue(4)]
            public ushort Value { get; set; }

            [PrimitiveValue(6)]
            public ushort Zero { get; set; }

            [PrimitiveArray(8,6)]
            public float[] Values{ get; set; }
        }

        
        

    }
}
