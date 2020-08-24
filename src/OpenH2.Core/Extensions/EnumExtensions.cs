using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenH2.Core.Extensions
{
    public static class EnumExtensions
    {
        public static IEnumerable<T> GetFlags<T>(this T value) where T: Enum
        {
            var allValues = Enum.GetValues(typeof(T));

            foreach(T v in allValues)
            {
                if(value.HasFlag(v))
                {
                    yield return v;
                }
            }
        }
    }
}
