using System;
using System.Text;

namespace OpenH2.Core.Extensions
{
    public unsafe static class EncodingExtensions
    {
        // TODO this only works on byte=char -> utf8/asci
        public static string GetNullTerminatedString(this Encoding encoding, byte* data, int maxLength = short.MaxValue)
        {
            var i = 0;
            for (; i < maxLength; i++)
            {
                if (data[i] == 0)
                    break;
            }

            return encoding.GetString(data, i);
        }

        // TODO this only works on byte=char -> utf8/asci
        public static string GetNullTerminatedString(this Encoding encoding, ReadOnlySpan<byte> data, int maxLength = short.MaxValue)
        {
            var i = 0;
            for (; i < maxLength; i++)
            {
                if (data[i] == 0)
                    break;
            }

            return encoding.GetString(data.Slice(0, i));
        }
    }
}
