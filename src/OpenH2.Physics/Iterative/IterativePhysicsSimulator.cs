using OpenH2.Physics.Abstractions;
using OpenH2.Physics.Collision;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenH2.Physics.Iterative
{
    public class IterativePhysicsSimulator : IPhysicsSimulator
    {
        private readonly int iterations;
        private IBroadPhaseDetector broadPhase;
        private INarrowPhaseDetector narrowPhase;

        public IterativePhysicsSimulator(int iterations)
        {
            this.iterations = iterations;
            this.broadPhase = new SweepAndPruneDetector();
            this.narrowPhase = new GjkDetector();
        }

        public object DetectCollisions(object world)
        {
            // Do broad phase to get candidates, need broad phase provider
            var candidates = broadPhase.DetectCandidateCollisions(/*world*/);

            // Do narrow phase to get actual collision data, need narrow phase provider
            return narrowPhase.DetectCollisions(candidates);
        }

        public void ResolveCollisions(object detectedCollisionData)
        {
            // Need objs with collision point, normal, object pair, material?

            for(var i = 0; i < iterations; i++)
            {
                // loop over collisions
                    // Execute solver(s?) to determine forces
                    // apply forces/torques to march towards resolution
            }

            // after iterations are done, objects will have been moved via applied forces, so physics is done
        }
    }
}
