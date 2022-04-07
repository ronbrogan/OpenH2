using OpenH2.Core.Offsets;
using OpenH2.Core.Maps;
using OpenH2.Core.Tags.Common.Collision;
using OpenH2.Core.Tags.Common.Models;
using OpenH2.Core.Tags.Layout;
using OpenBlam.Serialization.Layout;
using System;
using System.Numerics;
using System.Text.Json.Serialization;
using OpenBlam.Core.MapLoading;
using OpenBlam.Serialization;
using System.Diagnostics;
using OpenH2.Core.Extensions;

namespace OpenH2.Core.Tags
{
    [TagLabel(TagName.sbsp)]
    public partial class BspTag : BaseTag
    {
        [JsonIgnore]
        public byte[] RawMeta { get; set; }

        public override string Name { get; set; }

        public BspTag(uint id) : base(id)
        {
        }

        [PrimitiveValue(8)]
        public int Checksum { get; set; }

        [ReferenceArray(12)]
        public ShaderInfo[] Shaders { get; set; }

        [ReferenceArray(20)]
        public CollisionInfo[] CollisionInfos { get; set; }


        [PrimitiveValue(52)]
        public float MinX { get; set; }

        [PrimitiveValue(56)]
        public float MaxX { get; set; }

        [PrimitiveValue(60)]
        public float MinY { get; set; }

        [PrimitiveValue(64)]
        public float MaxY { get; set; }

        [PrimitiveValue(68)]
        public float MinZ { get; set; }

        [PrimitiveValue(72)]
        public float MaxZ { get; set; }


        //[InternalReferenceValue(76)]
        //public object[] MiscObject1Cao { get; set; }

        //[InternalReferenceValue(84)]
        //public object[] MiscObject2Cao { get; set; }

        [ReferenceArray(92)] 
        public Obj92[] Obj92s { get; set; }

        //[InternalReferenceValue(100)]
        //public Obj100[] Obj100s { get; set; } 

        //[InternalReferenceValue(148)]
        //public object[] MiscObject5Cao { get; set; } 

        [ReferenceArray(156)]
        public RenderChunk[] RenderChunks { get; set; }

        [ReferenceArray(164)]
        public ModelShaderReference[] ModelShaderReferences { get; set; }

        //[InternalReferenceValue(172)]
        //public object[] MiscObject8Cao { get; set; } 

        //[InternalReferenceValue(212)]
        //public object[] MiscObject9Cao { get; set; } 

        //[InternalReferenceValue(220)]
        //public object[] MiscObject10Cao { get; set; }

        [ReferenceArray(244)]
        public DecalInstance[] DecalInstances { get; set; }


        //[InternalReferenceValue(252)]
        //public object[] MiscObject11Cao { get; set; }

        //[InternalReferenceValue(260)]
        //public object[] MiscObject12Cao { get; set; }

        [ReferenceArray(312)]
        public InstancedGeometryDefinition[] InstancedGeometryDefinitions { get; set; }

        [ReferenceArray(320)] 
        public InstancedGeometryInstance[] InstancedGeometryInstances { get; set; }

        //[InternalReferenceValue(328)]
        //public object[] MiscObject15Cao { get; set; }

        //[InternalReferenceValue(336)]
        //public object[] MiscObject16Cao { get; set; }

        [ReferenceArray(344)]
        public Obj344[] Obj344s { get; set; }

        //[InternalReferenceValue(464)]
        //public object[] MiscObject18Cao { get; set; }

        //[InternalReferenceValue(480)]
        //public object[] MiscObject19Cao { get; set; }

        [ReferenceArray(524)] 
        public Obj524[] Obj524s { get; set; }


        //[InternalReferenceValue(540)]
        //public object[] MiscObject20Cao { get; set; }

        //[InternalReferenceValue(548)]
        //public object[] MiscObject21Cao { get; set; }

        //[InternalReferenceValue(556)]
        //public object[] MiscObject22Cao { get; set; }

        //[InternalReferenceValue(564)]
        //public object[] MiscObject23Cao { get; set; }


        public override void PopulateExternalData(MapStream reader)
        {
            

            foreach (var part in RenderChunks)
            {
                if (part.DataBlockRawOffset == uint.MaxValue)
                {
                    // TODO: determine why this happens, and if it's expected?
                    //Console.WriteLine("Bsp part with max DataBlock offset");
                    part.Model = new MeshCollection(new ModelMesh[0]);
                    continue;
                }

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

                var meshes = ModelResourceContainerProcessor.ProcessContainer(part, ModelShaderReferences);
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

                var headerOffset = new NormalOffset((int)def.DataBlockRawOffset);
                var mapData = reader.GetStream(headerOffset.Location);
                def.Header = BlamSerializer.Deserialize<ModelResourceBlockHeader>(mapData, headerOffset.Value);

                foreach (var resource in def.Resources)
                {
                    var dataOffset = def.DataBlockRawOffset + 8 + def.DataPreambleSize + resource.Offset;
                    mapData.Position = new NormalOffset((int)dataOffset).Value;

                    var resourceData = new byte[resource.Size];
                    var readCount = mapData.Read(resourceData, 0, resource.Size);

                    Debug.Assert(readCount == resource.Size);

                    resource.Data = resourceData;
                }

                var meshes = ModelResourceContainerProcessor.ProcessContainer(def, ModelShaderReferences, "InstancedGeometry_" + def.DataBlockRawOffset);
                def.Model = new MeshCollection(meshes);
            }
        }

        [FixedLength(20)]
        public class ShaderInfo
        {
            [PrimitiveValue(4)]
            public int Unknown { get; set; }

            [PrimitiveValue(8)]
            public ushort GlobalMaterialId { get; set; }

            [PrimitiveValue(16)]
            public TagRef<ShaderTag> Shader { get; set; }
        }

        [FixedLength(68)]
        public class CollisionInfo : ICollisionInfo
        {
            [ReferenceArray(0)]
            public Node3D[] Node3Ds { get; set; }

            [ReferenceArray(8)]
            public Common.Collision.Plane[] Planes { get; set; }

            [ReferenceArray(16)]
            public RawObject3[] RawObject3s { get; set; }

            //[ReferenceArray(24)]
            public RawObject4[] RawObject4s { get; set; }

            [ReferenceArray(32)]
            public Node2D[] Node2Ds { get; set; }

            [ReferenceArray(40)]
            public Face[] Faces { get; set; }

            [ReferenceArray(48)]
            public HalfEdgeContainer[] HalfEdges { get; set; }

            [ReferenceArray(56)]
            public Vertex[] Vertices { get; set; }

            [PrimitiveValue(64)]
            public int Unknown { get; set; }

            
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

            public ushort PartCount { get; set; } = 2;

            [PrimitiveValue(40)]
            public uint DataBlockRawOffset { get; set; }

            [PrimitiveValue(44)]
            public uint DataBlockSize { get; set; }

            [PrimitiveValue(48)]
            public uint DataPreambleSize { get; set; }

            [PrimitiveValue(52)]
            public uint ResourceSubsectionSize { get; set; }

            [ReferenceArray(56)]
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
        public class InstancedGeometryDefinition : IModelResourceContainer, ICollisionInfo
        {
            [PrimitiveValue(0)]
            public ushort VertexCount { get; set; }

            [PrimitiveValue(2)]
            public ushort TriangleCount { get; set; }

            [PrimitiveValue(12)]
            public ushort PartCount { get; set; }

            [ReferenceArray(24)]
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

            [ReferenceArray(56)]
            public ModelResource[] Resources { get; set; }
            public MeshCollection Model { get; set; }

            [ReferenceArray(112)]
            public Node3D[] Node3Ds { get; set; }

            [ReferenceArray(120)]
            public Common.Collision.Plane[] Planes { get; set; }

            [ReferenceArray(128)]
            public RawObject3[] RawObject3s { get; set; }

            //[ReferenceArray(136)]
            public RawObject4[] RawObject4s { get; set; }

            [ReferenceArray(144)]
            public Node2D[] Node2Ds { get; set; }

            [ReferenceArray(152)]
            public Face[] Faces { get; set; }

            [ReferenceArray(160)]
            public HalfEdgeContainer[] HalfEdges { get; set; }

            [ReferenceArray(168)]
            public Vertex[] Vertices { get; set; }

            [FixedLength(56)]
            public class Obj24
            {
                [PrimitiveArray(0, 10)]
                public float[] Floats { get; set; }
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
