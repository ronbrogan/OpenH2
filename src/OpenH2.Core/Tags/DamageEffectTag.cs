using OpenH2.Core.Tags.Layout;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenH2.Core.Tags
{
    [TagLabel(TagName.jpt)]
    public class DamageEffectTag : BaseTag
    {
        public DamageEffectTag(uint id) : base(id)
        {
        }
    }
}
