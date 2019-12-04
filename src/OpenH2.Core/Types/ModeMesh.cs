using OpenH2.Foundation;
using System;

namespace OpenH2.Core.Types
{
    public class ModeMesh
    {
        public uint ShaderCount { get; set; }
        public uint UnknownCount { get; set; }
        public uint IndiciesCount { get; set; }
        public uint BoneCount { get; set; }


        public Memory<byte>[] ShaderData { get; set; }
        
        public Memory<byte>[] UnknownData { get; set; }
        
        public int[] Indicies { get; set; }
        
        public VertexFormat[] Verticies { get; set; }


        public int ShaderChunkSize => 72;
        public int ShaderDataOffset => 120;

        public int UnknownChunkSize => 8;
        public int UnknownDataOffset => ShaderDataOffset + 4 + (int)(ShaderCount * ShaderChunkSize);

        public int IndiciesChunkSize => 4 + (int)(IndiciesCount * 2);
        public int IndiciesDataOffset => UnknownDataOffset + 4 + (int)(UnknownCount * UnknownChunkSize);

        public int VertexHeaderSize => 100;
        public int VertexDataOffset => IndiciesDataOffset + IndiciesChunkSize + VertexHeaderSize;
    }
}
