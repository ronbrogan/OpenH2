using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace OpenH2.Foundation.Physics
{
    public interface ISupportableCollider : ICollider
    {
        Vector3 Position { get; }
        Vector3 Support(Vector3 direction, out float distance);
    }
}
