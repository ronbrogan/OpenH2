using OpenH2.Physics.SpatialPartitioning;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenH2.Foundation.Physics
{
    public interface ILevelGeometry<TCollider>
    {
        TCollider[] Collision { get; }
    }
}
