using System.Text;

namespace OpenH2.Core.Extensions
{
    public unsafe static class EncodingExtensions
    {
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
    }
}
