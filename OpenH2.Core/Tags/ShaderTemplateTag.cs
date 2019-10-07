using OpenH2.Core.Tags.Layout;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenH2.Core.Tags
{
    [TagLabel("stem")]
    public class ShaderTemplateTag : BaseTag
    {
        public override string Name { get; set; }

        public ShaderTemplateTag(uint id) : base(id)
        {
        }


        [InternalReferenceValue(112)]
        public ShaderPassReference[] ShaderPassReferences { get; set; }

        [FixedLength(12)]
        public class ShaderPassReference
        {
            [PrimitiveValue(4)]
            public uint ShaderPassId { get; set; }
        }
    }
}
