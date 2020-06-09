using System;

namespace OpenH2.Foundation.Extensions
{
    public static class MathExt
    {
        public static float Clamp(float value, float min, float max)
        {
            return Math.Min(max, Math.Max(value, min));
        }
    }
}
