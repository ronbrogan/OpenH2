using System.Diagnostics;

namespace OpenH2.Core.Representations
{
    [DebuggerDisplay("\"{Value}\"")]
    public struct InternedString
    {
        public InternedString(uint id, uint length)
        {
            this.Id = id;
            this.Length = length;
            this.Value = null;
        }

        public uint Id { get; set; }

        public uint Length { get; set; }

        public string Value { get; set; }
    }
}
