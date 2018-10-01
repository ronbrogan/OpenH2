using System;

namespace OpenH2.Core.Parsing
{
    public delegate void SliceUsageCallback(int offset, int length, string label);

    public class TrackingChunk
    {
        private TrackingReader parser;
        private int start;
        private int length;
        private SliceUsageCallback logUsage;

        public Span<byte> Span => this.parser.Span.Slice(this.start, this.length);

        public TrackingChunk(TrackingReader parentParser, int start, int length, SliceUsageCallback usageCallback)
        {
            this.parser = parentParser;
            this.start = start;
            this.length = length;
            this.logUsage = usageCallback;

            this.logUsage(start, length, null);
        }

        public Span<byte> TrackedSlice(int offset, int length, string label = null)
        {
            var absoluteStart = this.start + offset;

            this.logUsage(absoluteStart, length, label);

            return this.parser.Span.Slice(absoluteStart, length);
        }

        public int Length => this.length;
    }
}