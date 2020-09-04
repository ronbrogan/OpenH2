using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace OpenH2.Serialization.Tests
{
    [TestClass]
    public partial class DeserializerTests
    {
        [TestMethod]
        public void DeserializeNestedTypes_FromSpan()
        {
            var tagStruct = (StructTestTag)BlamSerializer.Deserialize(typeof(StructTestTag), testTagData, 0, 100, null);
            AssertData(tagStruct);

            tagStruct = BlamSerializer.Deserialize<StructTestTag>(testTagData, 0, 100, null);
            AssertData(tagStruct);

            var tagRef = (ClassTestTag)BlamSerializer.Deserialize(typeof(ClassTestTag), testTagData, 0, 100, null);
            AssertData(tagRef);

            tagRef = BlamSerializer.Deserialize<ClassTestTag>(testTagData, 0, 100, null);
            AssertData(tagRef);


            var tagStructInstance = new StructTestTag();
            tagStructInstance = (StructTestTag)BlamSerializer.DeserializeInto(tagStructInstance, typeof(StructTestTag), testTagData, 0, 100, null);
            AssertData(tagStructInstance);

            tagStructInstance = new StructTestTag();
            tagStructInstance = BlamSerializer.DeserializeInto<StructTestTag>(tagStructInstance, testTagData, 0, 100, null);
            AssertData(tagStructInstance);

            var tagRefInstance = new ClassTestTag();
            tagRefInstance = (ClassTestTag)BlamSerializer.DeserializeInto(tagRefInstance, typeof(ClassTestTag), testTagData, 0, 100, null);
            AssertData(tagRefInstance);

            tagRefInstance = new ClassTestTag();
            tagRefInstance = BlamSerializer.DeserializeInto<ClassTestTag>(tagRefInstance, testTagData, 0, 100, null);
            AssertData(tagRefInstance);
        }

        [TestMethod]
        public void DeserializeNestedTypes_FromStream()
        {
            var tagStruct = (StructTestTag)BlamSerializer.Deserialize(typeof(StructTestTag), new MemoryStream(testTagData), 0, 100, null);
            AssertData(tagStruct);

            tagStruct = BlamSerializer.Deserialize<StructTestTag>(new MemoryStream(testTagData), 0, 100, null);
            AssertData(tagStruct);

            var tagRef = (ClassTestTag)BlamSerializer.Deserialize(typeof(ClassTestTag), new MemoryStream(testTagData), 0, 100, null);
            AssertData(tagRef);

            tagRef = BlamSerializer.Deserialize<ClassTestTag>(new MemoryStream(testTagData), 0, 100, null);
            AssertData(tagRef);


            var tagStructInstance = new StructTestTag();
            tagStructInstance = (StructTestTag)BlamSerializer.DeserializeInto(tagStructInstance, typeof(StructTestTag), new MemoryStream(testTagData), 0, 100, null);
            AssertData(tagStructInstance);

            tagStructInstance = new StructTestTag();
            tagStructInstance = BlamSerializer.DeserializeInto<StructTestTag>(tagStructInstance, new MemoryStream(testTagData), 0, 100, null);
            AssertData(tagStructInstance);

            var tagRefInstance = new ClassTestTag();
            tagRefInstance = (ClassTestTag)BlamSerializer.DeserializeInto(tagRefInstance, typeof(ClassTestTag), new MemoryStream(testTagData), 0, 100, null);
            AssertData(tagRefInstance);

            tagRefInstance = new ClassTestTag();
            tagRefInstance = BlamSerializer.DeserializeInto<ClassTestTag>(tagRefInstance, new MemoryStream(testTagData), 0, 100, null);
            AssertData(tagRefInstance);
        }

        private void AssertData(StructTestTag tag)
        {
            Assert.IsNotNull(tag);

            Assert.AreEqual(119, tag.Value1);
            Assert.AreEqual(151.251f, tag.Value2, 3);

            Assert.AreEqual(2, tag.SubValues.Length);

            Assert.AreEqual(0.001f, tag.SubValues[1].SubSubTags[1].Value, 3);

            Assert.AreEqual(0xefbeadde, tag.SubValues[0].ArrayItem[0]);

            Assert.AreEqual("mt1", tag.SubValues[0].SubSubTags[0].StringVal);
        }

        private void AssertData(ClassTestTag tag)
        {
            Assert.IsNotNull(tag);

            Assert.AreEqual(119, tag.Value1);
            Assert.AreEqual(151.251f, tag.Value2, 3);

            Assert.AreEqual(2, tag.SubValues.Length);

            Assert.AreEqual(0.001f, tag.SubValues[1].SubSubTags[1].Value, 3);

            Assert.AreEqual(0xefbeadde, tag.SubValues[0].ArrayItem[0]);

            Assert.AreEqual("mt1", tag.SubValues[0].SubSubTags[0].StringVal);
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
