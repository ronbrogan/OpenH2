using System;
using OpenH2.Core.Offsets;
using System.Collections.Generic;
using Newtonsoft.Json;
using OpenH2.Core.Tags.Layout;
using OpenH2.Core.Parsing;

namespace OpenH2.Core.Tags
{
    [TagLabel("sbsp")]
    public class Bsp : BaseTag
    {
        [JsonIgnore]
        public byte[] RawMeta { get; set; }

        public override string Name { get; set; }

        public Bsp(uint id) : base(id)
        {
        }

        [PrimitiveValue(8)]
        public int Checksum { get; set; }

        [InternalReferenceValue(12)]
        public ShaderInfo[] Shaders { get; set; }

        [InternalReferenceValue(20)]
        public CollisionInfo[] CollisionInfos { get; set; }


        //[InternalReferenceValue(76)]
        //public object[] MiscObject1Cao { get; set; }

        //[InternalReferenceValue(84)]
        //public object[] MiscObject2Cao { get; set; }

        //[InternalReferenceValue(92)]
        //public object[] MiscObject3Cao { get; set; }

        //[InternalReferenceValue(100)]
        //public object[] MiscObject4Cao { get; set; } 

        //[InternalReferenceValue(148)]
        //public object[] MiscObject5Cao { get; set; } 

        [InternalReferenceValue(156)]
        public RenderChunk[] RenderChunks { get; set; }

        //[InternalReferenceValue(164)]
        //public object[] MiscObject7Cao { get; set; } 

        //[InternalReferenceValue(172)]
        //public object[] MiscObject8Cao { get; set; } 

        //[InternalReferenceValue(212)]
        //public object[] MiscObject9Cao { get; set; } 

        //[InternalReferenceValue(220)]
        //public object[] MiscObject10Cao { get; set; }

        //[InternalReferenceValue(252)]
        //public object[] MiscObject11Cao { get; set; }

        //[InternalReferenceValue(260)]
        //public object[] MiscObject12Cao { get; set; }

        //[InternalReferenceValue(312)]
        //public object[] MiscObject13Cao { get; set; }

        //[InternalReferenceValue(320)]
        //public object[] MiscObject14Cao { get; set; }

        //[InternalReferenceValue(328)]
        //public object[] MiscObject15Cao { get; set; }

        //[InternalReferenceValue(336)]
        //public object[] MiscObject16Cao { get; set; }

        //[InternalReferenceValue(344)]
        //public object[] MiscObject17Cao { get; set; }

        //[InternalReferenceValue(464)]
        //public object[] MiscObject18Cao { get; set; }

        //[InternalReferenceValue(480)]
        //public object[] MiscObject19Cao { get; set; }

        //[InternalReferenceValue(540)]
        //public object[] MiscObject20Cao { get; set; }

        //[InternalReferenceValue(548)]
        //public object[] MiscObject21Cao { get; set; }

        //[InternalReferenceValue(556)]
        //public object[] MiscObject22Cao { get; set; }

        //[InternalReferenceValue(564)]
        //public object[] MiscObject23Cao { get; set; }


        public override void PopulateExternalData(TrackingReader sceneReader)
        {
            foreach(var chunk in RenderChunks)
            {
                var chunkResourceChunkStart = chunk.DataBlockRawOffset + 8 + chunk.DataPreambleSize;

                foreach(var resource in chunk.Resources)
                {
                    var data = sceneReader.Chunk((int)(chunkResourceChunkStart + resource.Offset), resource.Size, "Bsp Render Data");

                    resource.Data = data.AsMemory();
                }
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
            public uint ShaderId { get; set; }
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
        
        [FixedLength(176)]
        public class RenderChunk
        {
            [PrimitiveValue(0)]
            public ushort VertexCount { get; set; }

            [PrimitiveValue(2)]
            public ushort TriangleCount { get; set; }

            [PrimitiveValue(40)]
            public uint DataBlockRawOffset { get; set; }

            [PrimitiveValue(44)]
            public uint DataBlockSize { get; set; }

            [PrimitiveValue(48)]
            public uint DataPreambleSize { get; set; }

            [PrimitiveValue(52)]
            public uint ResourceSubsectionSize { get; set; }

            [InternalReferenceValue(56)]
            public Resource[] Resources { get; set; }

            [FixedLength(16)]
            public class Resource
            {
                [PrimitiveValue(0)]
                public ResourceType Type { get; set; }

                [PrimitiveValue(8)]
                public int Size { get; set; }

                [PrimitiveValue(12)]
                public int Offset { get; set; }

                public Memory<byte> Data { get; set; }

                [Flags]
                public enum ResourceType : byte
                {
                    One = 1,
                    Two = 2,
                    Three = 4
                }
            }
        }
    }
}
