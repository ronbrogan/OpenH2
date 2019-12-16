using OpenH2.Core.Tags;
using System;

namespace OpenH2.Core.Extensions
{
    public static class StringExtensions
    {
        public static string Reverse(this string s)
        {
            var c = s.ToCharArray();
            Array.Reverse(c);
            return new string(c);
        }

        public static bool IsSignificant(this string s)
        {
            return !string.IsNullOrWhiteSpace(s);
        }

        public static string ToTagString(this TagName tag)
        {
            if (Enum.IsDefined(typeof(TagName), tag))
            {
                return tag.ToString();
            }
            else
            {
                var u = (uint)tag;
                var chars = new char[4];

                for(var i = 0; i < 4; i++)
                {
                    chars[3-i] = (char)((u >> (i * 8)) & 0x000000FF);
                }

                return new string(chars);
            }
        }
    }
}