using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace OpenH2.Foundation.Physics
{
    public interface ISweepableBounds
    {
        Vector3 Min { get; }
        Vector3 Max { get; }
        float Radius { get; }
        Vector3 Center { get; }
    }
}
