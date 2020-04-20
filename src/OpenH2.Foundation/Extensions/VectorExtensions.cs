using System.Numerics;
using System.Runtime.CompilerServices;

namespace OpenH2.Foundation.Extensions
{
    public static class VectorExtensions
    {

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Axis(this Vector3 vec, int index)
        {
            return index == 0 ? vec.X : (index == 1 ? vec.Y : vec.Z);
        }
    }
}
