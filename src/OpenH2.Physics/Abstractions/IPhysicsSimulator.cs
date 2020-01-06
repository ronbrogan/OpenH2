using System;
using System.Collections.Generic;
using System.Text;

namespace OpenH2.Physics.Abstractions
{
    public interface IPhysicsSimulator
    {
        object DetectCollisions(object world);

        void ResolveCollisions(object detectedCollisionData);
    }
}
