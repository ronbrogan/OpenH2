using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenH2.Core.Parsing
{
    public class TrackingReader
    {
        public Memory<byte> Memory { get; }
        public Span<byte> Span => this.Memory.Span;
        private readonly Dictionary<ulong, string> Ranges = new Dictionary<ulong, string>();
        private const string DefaultLabel = "Unspecified";

        public TrackingReader(Memory<byte> data)
        {
            this.Memory = data;
        }

        /// <summary>
        /// Retrieve a span to original memory that has been sliced and tracked
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="label"></param>
        /// <returns>Slice of data as a span</returns>
        public Span<byte> Slice(int offset, int length, string label = null)
        {
            this.LogUsage(offset, length, label);

            return this.Memory.Span.Slice(offset, length);
        }

        /// <summary>
        /// Allocate a tracking chunk to retrieve values from data
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="length"></param>
        /// <param name="defaultLabel"></param>
        /// <returns></returns>
        public TrackingChunk Chunk(int offset, int length, string defaultLabel = null)
        {
            return new TrackingChunk(this, offset, length, (o, l, s) => this.LogUsage(o, l, s ?? defaultLabel));
        }

        internal CoverageReport GenerateReport()
        {
            var report = new CoverageReport();

            var ranges = this.GetRanges();

            var bytesCovered = 0;

            foreach(var range in ranges)
            {
                bytesCovered += (range.Key.Item2 - range.Key.Item1);
            }

            report.PercentCovered = (bytesCovered / (float)Memory.Length) * 100f;

            return report;
        }

        private void LogUsage(int offset, int length, string label)
        {
            if(string.IsNullOrEmpty(label))
            {
                label = DefaultLabel;
            }

            ulong key = (uint)offset;

            key = key << 32;
            key = key | (uint)(offset+length-1);

            if(this.Ranges.TryGetValue(key, out var existingValue))
            {
                this.Ranges[key] = existingValue + ";" + label;
            }
            else
            {
                this.Ranges[key] = label;
            }
        }

        public Dictionary<(int, int), string> GetRanges()
        {
            var keys = new ulong[this.Ranges.Keys.Count];
            this.Ranges.Keys.CopyTo(keys, 0);

            keys = keys.OrderBy(v => v).ToArray();

            var ranges = new Dictionary<(int, int), string>();

            foreach(var key in keys)
            {
                var begin = (int)((0xFFFFFFFF00000000 & key) >> 32);
                var end = (int)(0x00000000FFFFFFFF & key);

                ranges.Add((begin, end), this.Ranges[key]);
            }

            return ranges;
        }
    }
}