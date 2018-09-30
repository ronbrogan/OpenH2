using OpenH2.Core.Extensions;
using System;
using System.Linq;
using Xunit;

namespace OpenH2.Core.Tests.Extensions
{
    public class SpanByteExtensionTests
    {
        [Fact]
        public void Byte_array_to_string_with_null_terminator()
        {
            var stringData = "test\0".ToCharArray();

            var stringBytes = stringData.Select(c => Convert.ToByte(c)).ToArray();

            var stringSpan = new Span<byte>(stringBytes);

            var stringResult = stringSpan.ToStringFromNullTerminated();

            Assert.Equal("test", stringResult);
        }

        [Fact]
        public void Byte_array_to_string_with_null_terminator_and_garbage()
        {
            var stringData = "test\0ouasdfl;j\0".ToCharArray();

            var stringBytes = stringData.Select(c => Convert.ToByte(c)).ToArray();

            var stringSpan = new Span<byte>(stringBytes);

            var stringResult = stringSpan.ToStringFromNullTerminated();

            Assert.Equal("test", stringResult);
        }

        [Fact]
        public void Byte_array_to_string_without_null_terminator()
        {
            var stringData = "test".ToCharArray();

            var stringBytes = stringData.Select(c => Convert.ToByte(c)).ToArray();

            var stringSpan = new Span<byte>(stringBytes);

            var stringResult = stringSpan.ToStringFromNullTerminated();

            Assert.Equal("test", stringResult);
        }

        [Fact]
        public void Bytes_to_int()
        {
            var data = new byte[] { 0x00, 0x0E, 0xF5, 0x00};

            var span = new Span<byte>(data);

            var result = span.ReadInt32At(0);

            Assert.Equal(16059904, result);
        }
    }
}