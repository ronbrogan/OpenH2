using OpenH2.Core.Extensions;
using OpenH2.Core.Offsets;
using OpenH2.Core.Representations;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace OpenH2.Core.Parsing
{
    public class TrackingReader : IDisposable
    {
        public Stream Data { get; }
        private byte[] preloadData = new byte[81000];
        private int preloadStart = int.MaxValue;
        private int preloadLength = 0;

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
            Preload(offset, length);

            return new TrackingChunk(this, offset, length, (o, l, s) => this.LogUsage(o, l, s ?? defaultLabel));
        }


        public void Preload(int offset, int length)
        {
            var pLength = Math.Min(length, 81000);

            Data.Position = offset;

            Data.Read(preloadData, 0, pLength);
            preloadLength = pLength;
            preloadStart = offset;
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
            if(offset >= preloadStart && offset + length < preloadStart + preloadLength)
                return preloadData.AsSpan().ReadStringFrom(offset - preloadStart, length);

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
            if (offset >= preloadStart && offset + 1 < preloadStart + preloadLength)
                return preloadData[offset - preloadStart];

            return Data.ReadByteAt(offset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short ReadInt16At(int offset)
        {
            if (offset >= preloadStart && offset + 2 < preloadStart + preloadLength)
                return preloadData.AsSpan().ReadInt16At(offset - preloadStart);

            return Data.ReadInt16At(offset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReadInt32At(int offset)
        {
            if (offset >= preloadStart && offset + 4 < preloadStart + preloadLength)
                return preloadData.AsSpan().ReadInt32At(offset - preloadStart);

            return Data.ReadInt32At(offset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort ReadUInt16At(int offset)
        {
            if (offset >= preloadStart && offset + 2 < preloadStart + preloadLength)
                return preloadData.AsSpan().ReadUInt16At(offset - preloadStart);

            return Data.ReadUInt16At(offset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint ReadUInt32At(int offset)
        {
            if (offset >= preloadStart && offset + 4 < preloadStart + preloadLength)
                return preloadData.AsSpan().ReadUInt32At(offset - preloadStart);

            return Data.ReadUInt32At(offset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TagRef ReadTagRefAt(int offset)
        {
            if (offset >= preloadStart && offset + 4 < preloadStart + preloadLength)
                return preloadData.AsSpan().ReadTagRefAt(offset - preloadStart);

            return Data.ReadTagRefAt(offset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public InternedString ReadInternedStringAt(int offset)
        {
            if (offset >= preloadStart && offset + 4 < preloadStart + preloadLength)
                return preloadData.AsSpan().ReadInternedStringAt(offset - preloadStart);

            return Data.ReadInternedStringAt(offset);
        }

        public CountAndOffset ReadMetaCaoAt(int offset, TagIndexEntry index)
        {
            if (offset >= preloadStart && offset + 8 < preloadStart + preloadLength)
                return preloadData.AsSpan().ReadMetaCaoAt(offset - preloadStart, index);

            return Data.ReadMetaCaoAt(offset, index);
        }

        public CountAndOffset ReadMetaCaoAt(int offset, int magic)
        {
            if (offset >= preloadStart && offset + 8 < preloadStart + preloadLength)
                return preloadData.AsSpan().ReadMetaCaoAt(offset - preloadStart, magic);

            return Data.ReadMetaCaoAt(offset, magic);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2 ReadVec2At(int offset)
        {
            if (offset >= preloadStart && offset + 8 < preloadStart + preloadLength)
                return preloadData.AsSpan().ReadVec2At(offset - preloadStart);

            return Data.ReadVec2At(offset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3 ReadVec3At(int offset)
        {
            if (offset >= preloadStart && offset + 12 < preloadStart + preloadLength)
                return preloadData.AsSpan().ReadVec3At(offset - preloadStart);

            return Data.ReadVec3At(offset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector4 ReadVec4At(int offset)
        {
            if (offset >= preloadStart && offset + 16 < preloadStart + preloadLength)
                return preloadData.AsSpan().ReadVec4At(offset - preloadStart);

            return Data.ReadVec4At(offset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Quaternion ReadQuaternionAt(int offset)
        {
            if (offset >= preloadStart && offset + 16 < preloadStart + preloadLength)
                return preloadData.AsSpan().ReadQuaternionAt(offset - preloadStart);

            return Data.ReadQuaternionAt(offset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Matrix4x4 ReadMatrix4x4At(int offset)
        {
            if (offset >= preloadStart && offset + 64 < preloadStart + preloadLength)
                return preloadData.AsSpan().ReadMatrix4x4At(offset - preloadStart);

            return Data.ReadMatrix4x4At(offset);
        }

        public byte[] ReadArray(int offset, int length)
        {
            if (offset >= preloadStart && offset + length < preloadStart + preloadLength)
                return preloadData.AsSpan().Slice(offset - preloadStart, length).ToArray();

            return Data.ReadArray(offset, length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float ReadFloatAt(int offset)
        {
            if (offset >= preloadStart && offset + 4 < preloadStart + preloadLength)
                return preloadData.AsSpan().ReadFloatAt(offset - preloadStart);

            return Data.ReadFloatAt(offset);
        }

        public void Dispose()
        {
            this.Data?.Dispose();
        }
    }
}