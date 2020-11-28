using OpenH2.Core.Tags.Layout;
using OpenBlam.Serialization.Layout;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenH2.Core.Tags
{
    [TagLabel(TagName.NULL)]
    public class UnknownTag : BaseTag
    {
        public UnknownTag(uint id, string originalTag) : base(id)
        {
            this.OriginalLabel = originalTag;
        }

        public string OriginalLabel { get; }
    }
}
