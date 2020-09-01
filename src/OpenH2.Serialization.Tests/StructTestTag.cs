using OpenH2.Serialization.Layout;

namespace OpenH2.Serialization.Tests
{
    [ArbitraryLength]
    public struct StructTestTag
    {
        [PrimitiveValue(0)]
        public int Value1 { get; set; }

        [PrimitiveValue(4)]
        public float Value2 { get; set; }

        [ReferenceArray(8)]
        public SubTag[] SubValues { get; set; }

        [FixedLength(12)]
        public struct SubTag
        {
            [PrimitiveArray(0, 1)]
            public uint[] ArrayItem { get; set; }

            [ReferenceArray(4)]
            public SubSubTag[] SubSubTags { get; set; }

            [FixedLength(8)]
            public struct SubSubTag
            {
                [PrimitiveValue(0)]
                public float Value { get; set; }

                [StringValue(4, 4)]
                public string StringVal { get; set; }
            }
        }
    }

    [ArbitraryLength]
    public class ClassTestTag
    {
        [PrimitiveValue(0)]
        public int Value1 { get; set; }

        [PrimitiveValue(4)]
        public float Value2 { get; set; }

        [ReferenceArray(8)]
        public SubTag[] SubValues { get; set; }

        [FixedLength(12)]
        public class SubTag
        {
            [PrimitiveArray(0, 1)]
            public uint[] ArrayItem { get; set; }

            [ReferenceArray(4)]
            public SubSubTag[] SubSubTags { get; set; }

            [FixedLength(8)]
            public class SubSubTag
            {
                [PrimitiveValue(0)]
                public float Value { get; set; }

                [StringValue(4, 4)]
                public string StringVal { get; set; }
            }
        }
    }
}
