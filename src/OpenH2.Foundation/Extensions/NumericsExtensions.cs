using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace OpenH2.Foundation.Extensions
{
    public static class NumericsExtensions
    {
        public static float Get(this Vector3 vector, int i)
        {
            switch (i)
            {
                case 0:
                {
                    return vector.X;
                }
                case 1:
                {
                    return vector.Y;
                }
                case 2:
                {
                    return vector.Z;
                }
                default:
                {
                    throw new IndexOutOfRangeException(i.ToString());
                }
            }
        }
    }
}
