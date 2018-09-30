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
    }
}