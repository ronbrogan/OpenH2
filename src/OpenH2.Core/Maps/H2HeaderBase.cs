using OpenH2.Serialization.Layout;

namespace OpenH2.Core.Maps
{
    [SerializableType]
    public class H2HeaderBase
    {
        [StringValue(0, 4)]
        public string FileHead { get; set; }

        [PrimitiveValue(4)]
        public int Version { get; set; }
    }
}
