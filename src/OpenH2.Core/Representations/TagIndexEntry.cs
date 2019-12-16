using OpenH2.Core.Extensions;
using OpenH2.Core.Offsets;
using OpenH2.Core.Tags;

namespace OpenH2.Core.Representations
{
    public class TagIndexEntry
    {
        public TagName Tag { get; set; }
        public uint ID { get; set; }
        public IOffset Offset { get; set; }
        public int DataSize { get; set; }

        private string TagName => this.Tag.ToTagString();
        

        public static int Size => 16;

        public override string ToString()
        {
            return TagName + " " + ID + ", " + DataSize + "B @ " + Offset.Value;
        }
    }
}