using System.Diagnostics;

namespace OpenH2.Core.Representations
{
    public struct InternedString
    {
        public InternedString(uint id, uint length)
        {
            this.Id = id;
            this.Length = length;
        }

        public uint Id;

        public uint Length;
    }
}
