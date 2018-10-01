using OpenH2.Core.Enums;

namespace OpenH2.Core.Offsets
{
    public class NormalOffset : IOffset
    {
        private int offset;

        public NormalOffset(int offset)
        {
            this.offset = offset;
        }

        public int Value => this.offset & 0x3FFFFFFF;

        public int OriginalValue => this.offset;

        public DataFile Location => (DataFile)((this.offset & 0xC0000000) >> 28);
    }
}