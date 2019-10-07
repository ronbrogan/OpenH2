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

        [InternalReferenceValue(88)]
        public ShaderInfo[] ShaderInfos { get; set; }

        [FixedLength(40)]
        public class ShaderInfo
        {
            [InternalReferenceValue(0)]
            public Unknown1[] Unknown1s { get; set; }

            [InternalReferenceValue(8)]
            public short[] Unknown2s { get; set; }

            [InternalReferenceValue(16)]
            public ShaderPassReference[] ShaderPassReferences { get; set; }

            [InternalReferenceValue(24)]
            public Unknown4[] Unknown4s { get; set; }

            [InternalReferenceValue(32)]
            public Unknown5[] Unknown5s { get; set; }
        }

        [FixedLength(12)]
        public class Unknown1
        {
            [PrimitiveArray(0, 6)]
            public ushort[] Data { get; set; }
        }

        [FixedLength(12)]
        public class ShaderPassReference
        {
            [PrimitiveValue(4)]
            public uint ShaderPassId { get; set; }
        }

        [FixedLength(12)]
        public class Unknown4
        {
            [PrimitiveArray(0, 3)]
            public ushort[] Data { get; set; }
        }

        [FixedLength(12)]
        public class Unknown5
        {
            [PrimitiveValue(0)]
            public byte ValueA { get; set; }

            [PrimitiveValue(1)]
            public ushort ValueB { get; set; }
        }
    }
}
