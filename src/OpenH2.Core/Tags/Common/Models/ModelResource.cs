using OpenBlam.Serialization.Layout;
using System;
using System.Text.Json.Serialization;

namespace OpenH2.Core.Tags.Common.Models
{
    [FixedLength(16)]
    public class ModelResource
    {
        [PrimitiveValue(0)]
        public ResourceType Type { get; set; }

        [PrimitiveValue(8)]
        public int Size { get; set; }

        [PrimitiveValue(12)]
        public int Offset { get; set; }

        [JsonIgnore]
        public Memory<byte> Data { get; set; }

        [Flags]
        public enum ResourceType : byte
        {
            One = 1,
            VertexAttribute = 2,
            Three = 4
        }
    }
}
