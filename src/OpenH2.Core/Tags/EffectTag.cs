using OpenH2.Core.Tags.Layout;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenH2.Core.Tags
{
    [TagLabel(TagName.effe)]
    public class EffectTag : BaseTag
    {
        public EffectTag(uint id) : base(id)
        {
        }
    }
}
