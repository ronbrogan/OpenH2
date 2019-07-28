using OpenH2.Core.Tags.Layout;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenH2.Core.Tags
{
    [TagLabel("shad")]
    public class Shader : BaseTag
    {
        public override string Name { get; set; }

        public Shader(uint id) : base(id)
        {
        }

        [StringValue(0, 4)]
        public string StemTag { get; set; }

        [PrimitiveValue(4)]
        public uint StemTagId { get; set; }

        [InternalReferenceValue(12)]
        public BitmapInfo[] BitmapInfos { get; set; }

        [InternalReferenceValue(32)]
        public ParameterSet[] Parameters { get; set; }

        [InternalReferenceValue(44)]
        public BitmapReferenceSetting[] BitmapReferenceSettings { get; set; }

        [FixedLength(80)]
        public class BitmapInfo
        {
            [PrimitiveValue(4)]
            public uint DiffuseBitmapId { get; set; }

            [PrimitiveValue(12)]
            public uint EmissiveBitmapId { get; set; }

            [PrimitiveValue(16)]
            public float Param1 { get; set; }

            [PrimitiveValue(20)]
            public float Param2 { get; set; }

            [PrimitiveValue(24)]
            public float Param3 { get; set; }

            [PrimitiveValue(28)]
            public float Param4 { get; set; }
        }

        [FixedLength(124)]
        public class ParameterSet
        {
            [PrimitiveValue(0)]
            public uint Id { get; set; }

            [InternalReferenceValue(4)]
            public BitmapParameter1[] BitmapParameter1s { get; set; }

            //[ExternalReferenceValue(12)]

            [InternalReferenceValue(20)]
            public BitmapParameter2[] BitmapParamter2s { get; set; }
        }

        [FixedLength(12)]
        public class BitmapParameter1
        {
            [PrimitiveValue(0)]
            public uint BitmapId { get; set; }

            [PrimitiveValue(4)]
            public float ValueA { get; set; }

            [PrimitiveValue(8)]
            public float ValueB { get; set; }
        }

        [FixedLength(16)]
        public class BitmapParameter2
        {
            [PrimitiveValue(0)]
            public float ValueA { get; set; }

            [PrimitiveValue(4)]
            public float ValueB { get; set; }

            [PrimitiveValue(8)]
            public float ValueC { get; set; }

            [PrimitiveValue(12)]
            public float ValueD { get; set; }
        }

        [FixedLength(8)]
        public class BitmapReferenceSetting
        {
            [PrimitiveValue(0)]
            public short ValueA { get; set; }

            [PrimitiveValue(2)]
            public short ValueB { get; set; }

            [PrimitiveValue(4)]
            public uint BitmapId { get; set; }
        }
    }
}
