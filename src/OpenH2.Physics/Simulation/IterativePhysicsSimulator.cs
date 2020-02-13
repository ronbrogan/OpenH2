using OpenH2.Foundation.Physics;
using OpenH2.Physics.Abstractions;
using OpenH2.Physics.Colliders.Contacts;
using OpenH2.Physics.Collision;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenH2.Physics.Simulation
{
    public class IterativePhysicsSimulator : IPhysicsSimulator
    {
        private readonly int iterations;
        private IBroadPhaseDetector broadPhase;
        private IContactGenerator narrowPhase;
        private ContactResolver contactResolver;

        public IterativePhysicsSimulator(int iterations)
        {
            this.iterations = iterations;
            this.broadPhase = new SweepAndPruneDetector();
            this.narrowPhase = new TestContactGenerator();
            this.contactResolver = new ContactResolver(100);
        }

        public Contact[] DetectCollisions(IPhysicsWorld world, IList<IBody> bodies)
        {
            // Do broad phase to get candidates, need broad phase provider
            var candidates = broadPhase.DetectCandidateCollisions(world, bodies);

            // Do narrow phase to get actual collision data, need narrow phase provider
            return narrowPhase.DetectCollisions(candidates);
        }

        public void ResolveCollisions(Contact[] detectedCollisionData, double timestep)
        {
            var factor = timestep / iterations;

            for(var i = 0; i < iterations; i++)
            {
                contactResolver.ResolveContacts(detectedCollisionData, (float)factor);
            }
        }
    }
}
