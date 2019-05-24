using OpenH2.Core.Tags.Layout;
using OpenH2.Core.Tags.Processors;
using Xunit;

namespace OpenH2.Core.Tests.Tags
{
    public class TagFormatReaderGeneratorTests
    {
        [Fact]
        public void TestTag_DeserializesAllProperties()
        {
            var magic = 100;

            var creator = TagCreatorGenerator.GetTagCreator<TestTag>();

            var tag = creator(testTagData, magic);

            Assert.NotNull(tag);

            Assert.Equal(119, tag.Value1);
            Assert.Equal(151.251f, tag.Value2, 3);

            Assert.Equal(2, tag.SubValues.Length);

            Assert.Equal(0.001f, tag.SubValues[1].SubSubTags[1].Value, 3);
        }

        private class TestTag
        {
            [PrimitiveValue(0)]
            public int Value1 { get; set; }

            [PrimitiveValue(4)]
            public float Value2 { get; set; }

            [InternalReferenceValue(8)]
            public SubTag[] SubValues { get; set; }


            [FixedLength(12)]
            public class SubTag
            {
                [PrimitiveValue(0)]
                public int Deadbeef { get; set; }

                [InternalReferenceValue(4)]
                public SubSubTag[] SubSubTags { get; set; }

                [FixedLength(4)]
                public class SubSubTag
                {
                    [PrimitiveValue(0)]
                    public float Value { get; set; }
                }
            }
        }

        private byte[] testTagData = new byte[] {
            0x77, 0x00, 0x00, 0x00, 0x3F, 0x40, 0x17, 0x43, 0x02, 0x00, 0x00, 0x00, 0x7C, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xDE, 0xAD, 0xBE, 0xEF, 0x01, 0x00, 0x00, 0x00,
            0x94, 0x00, 0x00, 0x00, 0xDE, 0xAD, 0xBE, 0xEF, 0x02, 0x00, 0x00, 0x00, 0x98, 0x00, 0x00, 0x00,
            0x29, 0x1C, 0x18, 0xC3, 0x05, 0x34, 0xD9, 0x3F, 0x6F, 0x12, 0x83, 0x3A
        };
    }
}
