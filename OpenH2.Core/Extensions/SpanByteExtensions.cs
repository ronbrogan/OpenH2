using System;
using System.Text;

namespace OpenH2.Core.Extensions
{
    public static class SpanByteExtensions
    {
        public static string ToStringFromNullTerminated(this Span<byte> data)
        {
            var builder = new StringBuilder(data.Length);

            var current = 0;
            while(true)
            {
                if(current == data.Length || data[current] == 0b0)
                {
                    break;
                }

                builder.Append(Convert.ToChar(data[current]));
                current++;
            }

            return builder.ToString();
        }


        public static string StringFromSlice(this Span<byte> data, int offset, int length)
        {
            return data.Slice(offset, length).ToStringFromNullTerminated();
        }

        public static int IntFromSlice(this Span<byte> data, int offset)
        {
            var bytes = data.Slice(offset, 4);

            var value = 0;
            var shift = 0;

            foreach (int b in bytes)
            {
                // Shift bits into correct position and add into value
                value = value | (b << (shift * 8));

                shift++;
            }

            return value;
        }
    }
}
