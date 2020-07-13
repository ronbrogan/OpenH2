using OpenH2.Physics.Proxying;
using System.Collections.Concurrent;
using System.Numerics;

namespace OpenH2.Engine.Systems.Physics
{
    public class PhysicsForceQueue
    {
        //public const int DefaultQueueSize = 100;

        public ConcurrentBag<(IPhysicsProxy, Vector3)> VelocityChanges { get; set; } = new ConcurrentBag<(IPhysicsProxy, Vector3)>();

        public ConcurrentBag<(IPhysicsProxy, Vector3)> ForceChanges { get; set; } = new ConcurrentBag<(IPhysicsProxy, Vector3)>();

        public void Clear()
        {
            VelocityChanges.Clear();
            ForceChanges.Clear();
        }
    }
}
