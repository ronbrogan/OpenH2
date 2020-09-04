using OpenH2.Core.Offsets;
using OpenH2.Core.Representations;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace OpenH2.Core.Parsing
{
    public delegate void SliceUsageCallback(int offset, int length, string label);

    public class TrackingChunk
    {
        private TrackingReader parser;
        private int start;
        private int length;
        private SliceUsageCallback logUsage;

        public TrackingChunk(TrackingReader parentParser, int start, int length, SliceUsageCallback usageCallback)
        {
            this.parser = parentParser;
            this.start = start;
            this.length = length;
            this.logUsage = usageCallback;

            this.logUsage(start, length, null);
        }

        public int Length => this.length;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ReadStringFrom(int offset, int length)
        {
            return parser.ReadStringFrom(this.start + offset, length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ReadStringStarting(int offset)
        {
            return parser.ReadStringStarting(this.start + offset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public byte ReadByteAt(int offset)
        {
            return parser.ReadByteAt(this.start + offset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public short ReadInt16At(int offset)
        {
            return parser.ReadInt16At(this.start + offset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int ReadInt32At(int offset)
        {
            return parser.ReadInt32At(this.start + offset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ushort ReadUInt16At(int offset)
        {
            return parser.ReadUInt16At(this.start + offset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public uint ReadUInt32At(int offset)
        {
            return parser.ReadUInt32At(this.start + offset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TagRef ReadTagRefAt(int offset)
        {
            return parser.ReadTagRefAt(this.start + offset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2 ReadVec2At(int offset)
        {
            return parser.ReadVec2At(this.start + offset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3 ReadVec3At(int offset)
        {
            return parser.ReadVec3At(this.start + offset);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector4 ReadVec4At(int offset)
        {
            return parser.ReadVec4At(this.start + offset);
        }

        public byte[] ReadArray(int offset, int length)
        {
            return parser.ReadArray(this.start + offset, length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public float ReadFloatAt(int offset)
        {
            return parser.ReadFloatAt(this.start + offset);
        }
    }
}