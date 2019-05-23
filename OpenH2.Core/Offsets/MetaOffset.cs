using OpenH2.Core.Representations;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

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

        public int Value { get; private set; }

        public int OriginalValue { get; private set; }
    }
}
