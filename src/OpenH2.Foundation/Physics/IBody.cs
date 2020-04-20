using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace OpenH2.Foundation.Physics
{
    public interface IBody
    {
        void Wake();
        bool IsAwake { get; }
        bool IsStatic { get; }
        ICollider Collider { get; }
        ITransform Transform { get; }
        ISweepableBounds Bounds { get; }
        
    }
}
