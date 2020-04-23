namespace OpenH2.Core.Tags.Common.Models
{
    public class ModelResourceBlockHeader
    {
        public uint PartInfoCount { get; set; }

        public uint PartInfo2Count { get; set; }

        public uint PartInfo3Count { get; set; }

        public uint IndexCount { get; set; }

        public uint UknownDataLength { get; set; }

        public uint UknownIndiciesCount { get; set; }

        public uint VertexComponentCount { get; set; }
    }
}
