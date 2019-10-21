using OpenH2.Core.Extensions;
using OpenH2.Core.Offsets;
using OpenH2.Core.Representations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace OpenH2.Core.Parsing
{
    public class TrackingReader : IDisposable
    {
        public Stream Data { get; }
        private readonly Dictionary<ulong, string> Ranges = new Dictionary<ulong, string>();
        private const string DefaultLabel = "Unspecified";

        public TrackingReader(Stream Data)
        {
            this.Data = Data;
        }

        /// <summary>
        /// Allocate a tracking chunk to retrieve values from Data
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

            foreach (var range in ranges)
            {
                bytesCovered += (range.Key.Item2 - range.Key.Item1);
            }

            report.PercentCovered = (bytesCovered / (float)Data.Length) * 100f;

            return report;
        }

        private void LogUsage(int offset, int length, string label)
        {
            if (string.IsNullOrEmpty(label))
            {
                label = DefaultLabel;
            }

            ulong key = (uint)offset;

            key = key << 32;
            key = key | (uint)(offset + length - 1);

            if (this.Ranges.TryGetValue(key, out var existingValue))
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

            foreach (var key in keys)
            {
                var begin = (int)((0xFFFFFFFF00000000 & key) >> 32);
                var end = (int)(0x00000000FFFFFFFF & key);

                ranges.Add((begin, end), this.Ranges[key]);
            }

            return ranges;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ReadStringFrom(int offset, int length)
        {
            return Data.ReadStringFrom(offset, length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ReadStringStarting(int offset)
        {
            return Data.ReadStringStarting(offset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte ReadByteAt(int offset)
        {
            return Data.ReadByteAt(offset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short ReadInt16At(int offset)
        {
            return Data.ReadInt16At(offset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReadInt32At(int offset)
        {
            return Data.ReadInt32At(offset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort ReadUInt16At(int offset)
        {
            return Data.ReadUInt16At(offset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint ReadUInt32At(int offset)
        {
            return Data.ReadUInt32At(offset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TagRef ReadTagRefAt(int offset)
        {
            return Data.ReadTagRefAt(offset);
        }

        public CountAndOffset ReadMetaCaoAt(int offset, TagIndexEntry index)
        {
            return Data.ReadMetaCaoAt(offset, index);
        }

        public CountAndOffset ReadMetaCaoAt(int offset, int magic)
        {
            return Data.ReadMetaCaoAt(offset, magic);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2 ReadVec2At(int offset)
        {
            return Data.ReadVec2At(offset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3 ReadVec3At(int offset)
        {
            return Data.ReadVec3At(offset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector4 ReadVec4At(int offset)
        {
            return Data.ReadVec4At(offset);
        }

        public byte[] ReadArray(int offset, int length)
        {
            return Data.ReadArray(offset, length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float ReadFloatAt(int offset)
        {
            return Data.ReadFloatAt(offset);
        }

        public void Dispose()
        {
            this.Data?.Dispose();
        }
    }
}