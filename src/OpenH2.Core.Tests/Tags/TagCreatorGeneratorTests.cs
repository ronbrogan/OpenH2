using OpenH2.Core.Parsing;
using OpenH2.Core.Representations;
using OpenH2.Core.Tags;
using OpenH2.Core.Tags.Layout;
using OpenH2.Core.Tags.Serialization;
using OpenH2.Serialization;
using OpenH2.Serialization.Layout;
using System.IO;
using System.Reflection.Emit;
using Xunit;

namespace OpenH2.Core.Tests.Tags
{
    public class TagFormatReaderGeneratorTests
    {
        [Fact]
        public void TestTag_DeserializesAllProperties()
        {
            var map = new TestMap(secondaryMagic: 100);

            var gen = new TagCreatorGenerator();

            var creator = gen.GetTagCreator<TestTag>();

            var reader = new TrackingReader(new MemoryStream(testTagData));

            var tag = creator(1, "name", reader, map, 0, testTagData.Length);

            tag.PopulateExternalData(null);

            Assert.NotNull(tag);

            Assert.Equal(1u, tag.Id);
            Assert.Equal("name", tag.Name);

            Assert.Equal(119, tag.Value1);
            Assert.Equal(151.251f, tag.Value2, 3);

            Assert.Equal(2, tag.SubValues.Length);

            Assert.Equal(0.001f, tag.SubValues[1].SubSubTags[1].Value, 3);

            Assert.Equal(0xefbeadde, tag.SubValues[0].ArrayItem[0]);

            Assert.NotNull(tag.FirstSubValue);

            Assert.Equal("mt1", tag.FirstSubValue.SubSubTags[0].StringVal);
        }

        [Fact]
        public void TestTag_BlamDeserializePropertiesOnly()
        {
            var tag = BlamSerializer.Deserialize<TestTag>(testTagData, 0, 100, null);
            tag.PopulateExternalData(null);

            Assert.NotNull(tag);

            Assert.Equal(119, tag.Value1);
            Assert.Equal(151.251f, tag.Value2, 3);

            Assert.Equal(2, tag.SubValues.Length);

            Assert.Equal(0.001f, tag.SubValues[1].SubSubTags[1].Value, 3);

            Assert.Equal(0xefbeadde, tag.SubValues[0].ArrayItem[0]);

            Assert.NotNull(tag.FirstSubValue);

            Assert.Equal("mt1", tag.FirstSubValue.SubSubTags[0].StringVal);
        }

        private class TestMap : H2vBaseMap
        {
            public TestMap(int secondaryMagic)
            {
                this.SecondaryMagic = secondaryMagic;
            }
        }

        [ArbitraryLength]
        public class TestTag : BaseTag
        {
            public override string Name { get; set; }

            public TestTag(uint id) : base(id)
            {

            }

            [PrimitiveValue(0)]
            public int Value1 { get; set; }

            [PrimitiveValue(4)]
            public float Value2 { get; set; }

            [ReferenceArray(8)]
            public SubTag[] SubValues { get; set; }

            public SubTag FirstSubValue { get; set; }

            public override void PopulateExternalData(H2vReader sceneReader)
            {
                FirstSubValue = SubValues[0];
            }

            [FixedLength(12)]
            public class SubTag
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

        private byte[] testTagData = new byte[] {
            0x77, 0x00, 0x00, 0x00, 0x3F, 0x40, 0x17, 0x43, 0x02, 0x00, 0x00, 0x00, 0x7C, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0xDE, 0xAD, 0xBE, 0xEF, 0x01, 0x00, 0x00, 0x00,
            0x94, 0x00, 0x00, 0x00, 0xDE, 0xAD, 0xBE, 0xEF, 0x02, 0x00, 0x00, 0x00, 0x9C, 0x00, 0x00, 0x00,
            0x29, 0x1C, 0x18, 0xC3, 0x6D, 0x74, 0x31, 0x00, 0x05, 0x34, 0xD9, 0x3F, 0x6D, 0x74, 0x69, 0x32,
            0x6F, 0x12, 0x83, 0x3A, 0x6D, 0x74, 0x69, 0x33
        };
    }
}
