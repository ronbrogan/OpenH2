using OpenH2.Core.Offsets;

namespace OpenH2.Core.Representations
{
    public class TagIndexEntry
    {
        public string Tag { get; set; }
        public uint ID { get; set; }
        public IOffset Offset { get; set; }
        public int MetaSize { get; set; }

        public static int Size => 16;
    }
}