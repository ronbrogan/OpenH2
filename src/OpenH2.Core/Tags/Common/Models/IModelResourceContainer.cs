using System;

namespace OpenH2.Core.Tags.Common.Models
{
    public enum GeometryClass : ushort
    {
        Worldspace,
        Rigid,
        RigidBoned,
        Skinned,
        UnsupportedBroken
    }

    [Flags]
    public enum GeometryCompressionFlags : short
    {
        CompressedVerts = 1 << 0,
        CompressedTexCoords = 1 << 1,
        CompressedSecTexCoords = 1 << 2,
    }

    public interface IModelResourceContainer
    {
        ushort VertexCount { get; set; }

        ushort TriangleCount { get; set; }

        ushort PartCount { get; set; }

        uint DataBlockRawOffset { get; set; }

        uint DataBlockSize { get; set; }

        uint DataPreambleSize { get; set; }

        GeometryClass GeometryClassification { get; set; }

        GeometryCompressionFlags CompressionFlags { get; set; }

        ModelResourceBlockHeader Header { get; set; }

        ModelResource[] Resources { get; set; }
    }
}
