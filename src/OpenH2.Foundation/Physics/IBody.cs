using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace OpenH2.Foundation.Physics
{
    public interface IBody
    {
        bool IsAwake { get; set; }
        bool IsStatic { get; }
        ICollider Collider { get; }
        ITransform Transform { get; }
    }
}
