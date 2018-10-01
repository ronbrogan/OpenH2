using System;
using System.IO;

namespace OpenH2.Core.Extensions
{
    public static class StreamExtensions
    {
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

        public static void WriteInt16(this Stream stream, int value)
        {
            stream.WriteByte((byte)value);
            stream.WriteByte((byte)(value >> 8));
        }
    }
}