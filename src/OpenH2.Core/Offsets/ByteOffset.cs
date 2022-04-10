using OpenH2.Core.Enums;

namespace OpenH2.Core.Offsets
{
    public struct ByteOffset : IOffset
    {
        private int offset;

        public ByteOffset(int offset)
        {
            this.offset = offset;
        }

        public int Value => this.offset;
        public int OriginalValue => this.offset;
    }
}
