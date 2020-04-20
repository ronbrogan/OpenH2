using OpenH2.Foundation.Numerics;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace OpenH2.Foundation.Physics
{
    public interface IVertexBasedCollider : ISupportableCollider
    {
        /// <summary>
        /// Gets un-transformed vertices (AKA in local space)
        /// </summary>
        Vector3[] Vertices { get; }

        short Support(short vertIndex, VectorD3 direction, out double distance);
    }
}
