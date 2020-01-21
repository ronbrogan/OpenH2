using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace OpenH2.Foundation.Physics
{
    public interface IPhysicsWorld
    {
        Vector3 Gravity { get; set; }
    }
}
