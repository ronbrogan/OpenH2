using OpenH2.Core.Tags.Layout;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenH2.Core.Tags
{
    [TagLabel("sky ")]
    public class SkyboxTag : BaseTag
    {
        public override string Name { get; set; }

        public SkyboxTag(uint id) : base(id)
        {
        }


        [PrimitiveValue(4)]
        public uint ModelTagId { get; set; }
    }
}
