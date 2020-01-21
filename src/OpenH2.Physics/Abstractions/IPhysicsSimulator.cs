using OpenH2.Foundation.Physics;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenH2.Physics.Abstractions
{
    public interface IPhysicsSimulator
    {
        Contact[] DetectCollisions(IPhysicsWorld world, IList<IBody> bodies);

        void ResolveCollisions(Contact[] detectedCollisionData, double timestep);
    }
}
