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
    }
}