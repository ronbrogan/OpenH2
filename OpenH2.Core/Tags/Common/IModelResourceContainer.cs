namespace OpenH2.Core.Tags.Common
{
    public interface IModelResourceContainer
    {
        ushort VertexCount { get; set; }

        ushort TriangleCount { get; set; }

        ushort IndiciesIndex { get; set; }

        uint DataBlockRawOffset { get; set; }

        uint DataBlockSize { get; set; }

        uint DataPreambleSize { get; set; }

        ModelResourceBlockHeader Header { get; set; }

        ModelResource[] Resources { get; set; }
    }
}
