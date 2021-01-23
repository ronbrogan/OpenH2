using System;

namespace OpenH2.Foundation.Extensions
{
    public static class MathExt
    {
        public static float Clamp(float value, float min, float max)
        {
            return Math.Min(max, Math.Max(value, min));
        }

        public static double ToRadians(double degrees)
        {
            return (degrees / 180.0 * Math.PI);
        }

        public static double ToDegrees(double radians)
        {
            return (radians * 180.0 / Math.PI);
        }
    }
}
