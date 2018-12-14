using OpenH2.Core.Offsets;
using System.Collections.Generic;

namespace OpenH2.Core.Meta
{
    public class BspMeta : BaseMeta
    {
        public override string Name { get; set; }

        public BspHeader Header { get; set; }
        public List<RawBlock> RawBlocks { get; internal set; }


        public byte[] RawMeta { get; set; }
        

        public class BspHeader
        {
            public int Checksum { get; set; }
            public CountAndOffset ShaderLocation { get; set; }
            public CountAndOffset RawBlockLocation { get; set; }
            public MiscBlockCaos MiscBlocks { get; set; }
            public List<ShaderInfo> Shaders { get; set; }
            public List<RawBlockCaos> RawBlocks { get; set; }
        }


        public class MiscBlockCaos
        {
            public CountAndOffset MiscObject1Cao { get; set; }
            public CountAndOffset MiscObject2Cao { get; set; }
            public CountAndOffset MiscObject3Cao { get; set; }
            public CountAndOffset MiscObject4Cao { get; set; }
            public CountAndOffset MiscObject5Cao { get; set; }
            public CountAndOffset MiscObject6Cao { get; set; }
            public CountAndOffset MiscObject7Cao { get; set; }
            public CountAndOffset MiscObject8Cao { get; set; }
            public CountAndOffset MiscObject9Cao { get; set; }
            public CountAndOffset MiscObject10Cao { get; set; }
            public CountAndOffset MiscObject11Cao { get; set; }
            public CountAndOffset MiscObject12Cao { get; set; }
            public CountAndOffset MiscObject13Cao { get; set; }
            public CountAndOffset MiscObject14Cao { get; set; }
            public CountAndOffset MiscObject15Cao { get; set; }
            public CountAndOffset MiscObject16Cao { get; set; }
            public CountAndOffset MiscObject17Cao { get; set; }
            public CountAndOffset MiscObject18Cao { get; set; }
            public CountAndOffset MiscObject19Cao { get; set; }
            public CountAndOffset MiscObject20Cao { get; set; }
            public CountAndOffset MiscObject21Cao { get; set; }
            public CountAndOffset MiscObject22Cao { get; set; }
            public CountAndOffset MiscObject23Cao { get; set; }
        }

        public class ShaderInfo
        {
            public string Tag { get; set; }
            public int Unknown { get; set; }
            public int Value1 { get; set; }
            public string OldTag { get; set; }
            public int Value2 { get; set; }

            public static int Length => 20;
        }

        public class RawBlockCaos
        {
            public CountAndOffset RawObject1Cao { get; set; }
            public CountAndOffset RawObject2Cao { get; set; }
            public CountAndOffset RawObject3Cao { get; set; }
            public CountAndOffset RawObject4Cao { get; set; }
            public CountAndOffset RawObject5Cao { get; set; }
            public CountAndOffset FacesCao { get; set; }
            public CountAndOffset HalfEdgeCao { get; set; }
            public CountAndOffset VerticiesCao { get; set; }
            public int Unknown { get; set; }

            public static int Length => 68;
        }

        public class RawBlock
        {
            public List<RawObject1> RawObject1s { get; set; } = new List<RawObject1>();
            public List<RawObject2> RawObject2s { get; set; } = new List<RawObject2>();
            public List<RawObject3> RawObject3s { get; set; } = new List<RawObject3>();
            public List<RawObject4> RawObject4s { get; set; } = new List<RawObject4>();
            public List<RawObject5> RawObject5s { get; set; } = new List<RawObject5>();
            public List<Face> Faces { get; set; } = new List<Face>();
            public List<HalfEdgeContainer> HalfEdges { get; set; } = new List<HalfEdgeContainer>();
            public List<Vertex> Verticies { get; set; } = new List<Vertex>();
        }

        public class RawObject1
        {
            public ushort val1;
            public ushort val2;
            public ushort unknown1;
            public ushort unknown2;

            public static int Length => 8;
        }

        public struct RawObject2
        {
            public float x;
            public float y;
            public float z;
            public float w;

            public static int Length => 16;
        }

        public class RawObject3
        {
            public ushort val1;
            public ushort val2;

            public static int Length => 4;
        }

        public class RawObject4
        {
            public ushort val1;
            public ushort val2;

            public static int Length => 4;
        }

        public struct RawObject5
        {
            public float x;
            public float y;
            public float z;
            public short u;
            public short v;

            public static int Length => 16;
        }

        public class Face
        {
            public ushort val1;
            public ushort FirstEdge;
            public ushort val3;
            public ushort val4;

            public static int Length => 8;
        }

        public class HalfEdgeContainer
        {
            public ushort Vertex0;
            public ushort Vertex1;
            public ushort NextEdge;
            public ushort PrevEdge;
            public ushort Face0;
            public ushort Face1;

            public static int Length => 12;
        }

        public struct Vertex
        {
            public float x;
            public float y;
            public float z;
            public int edge;

            public static int Length => 16;
        }
    }
}
