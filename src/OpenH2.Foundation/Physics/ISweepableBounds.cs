using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace OpenH2.Foundation.Physics
{
    public interface ISweepableBounds
    {
        Vector3 Min();
        Vector3 Max();
    }
}
