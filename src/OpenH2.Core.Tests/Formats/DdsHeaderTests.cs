using OpenH2.Core.Enums.Texture;
using OpenH2.TextureDumper;
using System;
using Xunit;
using Xunit.Abstractions;

namespace OpenH2.Core.Tests.Formats
{
    public class DdsHeaderTests
    {
        private ITestOutputHelper output;

        public DdsHeaderTests(ITestOutputHelper output)
        {
            this.output = output;
        }

        [Fact]
        public void DdsHeader_creates_128_length_stream()
        {
            var header = new DdsHeader(TextureFormat.DXT45, TextureType.TwoDimensional, 128, 128, 1, 1, null, 16384);

            Assert.Equal(128, header.HeaderData.Length);

            var data = new byte[128];
            header.HeaderData.Read(data);

            // Manual verification, paste into 010 Editor, run DDS template
            this.output.WriteLine("0x" + BitConverter.ToString(data).Replace("-", string.Empty));
        }

    }
}
