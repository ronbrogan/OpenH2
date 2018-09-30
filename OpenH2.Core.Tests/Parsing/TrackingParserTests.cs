using OpenH2.Core.Parsing;
using System;
using System.Linq;
using Xunit;

namespace OpenH2.Core.Tests.Parsing
{
    public class TrackingParserTests
    {
        [Fact]
        public void TrackingParser_slice()
        {
            var bytes = "testdata".Select(c => (byte)c).ToArray();
            var data = new Memory<byte>(bytes);
            var parser = new TrackingReader(data);

            parser.Slice(4, 3, "t");
            parser.Slice(0, 1, "d");

            var ranges = parser.GetRanges();

            Assert.Equal(2, ranges.Count);
            Assert.Equal("d", ranges[(0, 0)]);
            Assert.Equal("t", ranges[(4, 6)]);
        }
    }
}