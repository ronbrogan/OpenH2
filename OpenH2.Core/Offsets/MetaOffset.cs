using OpenH2.Core.Representations;
using System.Diagnostics;

namespace OpenH2.Core.Offsets
{
    /// <summary>
    /// This offset type represents a secondary offset within a tag block. 
    /// Because we do not have the context of the whole file while parsing 
    /// the tag, we can use the secondary as a local offset.
    /// </summary>
    [DebuggerDisplay("{Value}")]
    public class TagInternalOffset : IOffset
    {
        public TagInternalOffset(TagIndexEntry indexEntry, int offsetValue)
        {
            this.OriginalValue = offsetValue;

            this.Value = offsetValue - indexEntry.Offset.OriginalValue;
        }

        public TagInternalOffset(int magic, int offset)
        {
            this.OriginalValue = offset;
            this.Value = offset - magic;
        }

        public int Value { get; private set; }

        public int OriginalValue { get; private set; }
    }
}
