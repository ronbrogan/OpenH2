using OpenH2.Core.Offsets;
using OpenH2.Core.Representations;
using System;
using System.IO;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace OpenH2.Core.Extensions
{
    public static class StreamExtensions
    {
        [ThreadStatic]
        private static byte[] primitiveTempBuffer;

        private static byte[] PrimitiveTempBuffer
        {
            get
            {
                if (primitiveTempBuffer == null) primitiveTempBuffer = new byte[16];

                return primitiveTempBuffer;
            }
        }

        public static Memory<byte> ToMemory(this Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (stream.Length > int.MaxValue)
                throw new Exception("The stream provided is greater than " + int.MaxValue + " bytes long.");

            var bytes = new byte[stream.Length];
            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(bytes, 0, (int)stream.Length);
            return new Memory<byte>(bytes);
        }

        public static void WriteInt32(this Stream stream, int value)
        {
            stream.WriteByte((byte)value);
            stream.WriteByte((byte)(value >> 8));
            stream.WriteByte((byte)(value >> 16));
            stream.WriteByte((byte)(value >> 24));
        }

        public static void WriteUInt32(this Stream stream, uint value)
        {
            stream.WriteByte((byte)value);
            stream.WriteByte((byte)(value >> 8));
            stream.WriteByte((byte)(value >> 16));
            stream.WriteByte((byte)(value >> 24));
        }

        public static void WriteInt16(this Stream stream, int value)
        {
            stream.WriteByte((byte)value);
            stream.WriteByte((byte)(value >> 8));
        }



        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ToStringFromNullTerminated(this byte[] data)
        {
            var builder = new StringBuilder(data.Length);

            var current = 0;
            while (true)
            {
                if (current == data.Length || data[current] == 0b0)
                {
                    break;
                }

                builder.Append((char)data[current]);
                current++;
            }

            return builder.ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ReadStringFrom(this Stream data, int offset, int length)
        {
            var len = (int)Math.Min(length, data.Length - offset);

            return data.ReadArray(offset, len).ToStringFromNullTerminated();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string ReadStringStarting(this Stream data, int offset)
        {
            var builder = new StringBuilder(32);

            data.Position = offset;

            while (true)
            {
                if (data.ReadByte() == 0b0)
                {
                    break;
                }

                builder.Append((char)data.ReadByte());
            }

            return builder.ToString();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static byte ReadByteAt(this Stream data, int offset)
        {
            data.Position = offset;

            return (byte)data.ReadByte();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static short ReadInt16At(this Stream data, int offset)
        {
            if (offset + 2 > data.Length)
            {
                return 0;
            }

            data.Position = offset;
            data.Read(PrimitiveTempBuffer, 0, 2);

            uint value = PrimitiveTempBuffer[0] | (uint)(PrimitiveTempBuffer[1] << 8);

            return (short)value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int ReadInt32At(this Stream data, int offset)
        {
            if (offset + 4 > data.Length)
            {
                return 0;
            }

            data.Position = offset;

            data.Read(PrimitiveTempBuffer, 0, 4);

            int value = (PrimitiveTempBuffer[0])
               | (PrimitiveTempBuffer[1] << 8)
               | (PrimitiveTempBuffer[2] << 16)
               | (PrimitiveTempBuffer[3] << 24);

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ushort ReadUInt16At(this Stream data, int offset)
        {
            if (offset + 2 > data.Length)
            {
                return 0;
            }

            data.Position = offset;
            data.Read(PrimitiveTempBuffer, 0, 2);

            uint value = PrimitiveTempBuffer[0] | (uint)(PrimitiveTempBuffer[1] << 8);

            return (ushort)value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static uint ReadUInt32At(this Stream data, int offset)
        {
            if (offset + 4 > data.Length)
            {
                return 0;
            }

            data.Position = offset;

            data.Read(PrimitiveTempBuffer, 0, 4);

            uint value = (PrimitiveTempBuffer[0])
               | (uint)(PrimitiveTempBuffer[1] << 8)
               | (uint)(PrimitiveTempBuffer[2] << 16)
               | (uint)(PrimitiveTempBuffer[3] << 24);

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TagRef ReadTagRefAt(this Stream data, int offset)
        {
            return new TagRef(data.ReadUInt32At(offset));
        }

        public static CountAndOffset ReadMetaCaoAt(this Stream data, int offset, TagIndexEntry index)
        {
            return ReadMetaCaoAt(data, offset, index.Offset.Value);
        }

        public static CountAndOffset ReadMetaCaoAt(this Stream data, int offset, int magic)
        {
            return new CountAndOffset(data.ReadInt32At(offset), new SecondaryOffset(magic, data.ReadInt32At(offset + 4)));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 ReadVec2At(this Stream data, int offset)
        {
            return new Vector2(data.ReadFloatAt(offset), data.ReadFloatAt(offset + 4));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 ReadVec3At(this Stream data, int offset)
        {
            return new Vector3(data.ReadFloatAt(offset), data.ReadFloatAt(offset + 4), data.ReadFloatAt(offset + 8));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 ReadVec4At(this Stream data, int offset)
        {
            return new Vector4(data.ReadFloatAt(offset), data.ReadFloatAt(offset + 4), data.ReadFloatAt(offset + 8), data.ReadFloatAt(offset + 12));
        }

        public static byte[] ReadArray(this Stream data, int offset, int length)
        {
            data.Position = offset;
            var bytes = new byte[length];

            int current = 0;
            int remaining = length;
            while (remaining > 0)
            {
                int read = data.Read(bytes, current, remaining);

                if (read <= 0)
                    throw new EndOfStreamException(string.Format("End of stream reached with {0} bytes left to read", remaining));

                remaining -= read;
                current += read;
            }

            return bytes;
        }

        private static UInt32ToSingle floatConverter = new UInt32ToSingle();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float ReadFloatAt(this Stream data, int offset)
        {
            floatConverter.UInt32 = data.ReadUInt32At(offset);

            return floatConverter.Single;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct UInt32ToSingle
        {
            [FieldOffset(0)]
            public uint UInt32;

            [FieldOffset(0)]
            public float Single;
        }
    }
}