using OpenBlam.Serialization.Layout;

namespace OpenH2.Core.Tags.Common.Models
{
    [FixedLength(120)]
    public class ModelResourceBlockHeader
    {
        [PrimitiveValue(8)]
        public uint PartInfoCount { get; set; }

        [PrimitiveValue(16)]
        public uint PartInfo2Count { get; set; }

        [PrimitiveValue(24)]
        public uint PartInfo3Count { get; set; }

        [PrimitiveValue(40)]
        public uint IndexCount { get; set; }

        [PrimitiveValue(48)]
        public uint UknownDataLength { get; set; }

        [PrimitiveValue(56)]
        public uint UknownIndiciesCount { get; set; }

        [PrimitiveValue(64)]
        public uint VertexComponentCount { get; set; }
    }
}
