using OpenH2.Foundation.Physics;
using OpenH2.Physics.Abstractions;
using System.Collections.Generic;
using System.Numerics;

namespace OpenH2.Physics.Collision
{
    public class SweepAndPruneDetector : IBroadPhaseDetector
    {
        // PERF: find a better way to access vector components
        private int AxisSelector = 0;

        public SweepAndPruneDetector()
        {
        }

        // REF: Ericson p337
        public IList<IBody> DetectCandidateCollisions(IPhysicsWorld world, IList<IBody> bodies)
        {
            var candidates = new List<IBody>(bodies.Count);

            InPlaceBodySort(bodies);

            var s = new[] { 0f, 0f, 0f };
            var s2 = new[] { 0f, 0f, 0f };
            var v = new float[3];

            for(var i = 0; i < bodies.Count; i++)
            {
                var bounds = bodies[i].Bounds;

                var p = 0.5f * (bounds.Min + bounds.Max);

                s[0] += p.X;
                s2[0] += p.X * p.X;
                s[1] += p.Y;
                s2[1] += p.Y * p.Y;
                s[2] += p.Z;
                s2[2] += p.Z * p.Z;

                for (var j = i + 1; j < bodies.Count; j++)
                {

                    var a = bodies[j];
                    var b = bodies[i];

                    var aBounds = a.Bounds;
                    var bBounds = b.Bounds;

                    if(a.IsStatic && b.IsStatic)
                    {
                        continue;
                    }

                    if (SortAxis(aBounds.Min) > SortAxis(bBounds.Max))
                    {
                        break;
                    }

                    if(OverlapBounds(aBounds, bBounds))
                    {
                        candidates.Add(a);
                        candidates.Add(b);
                    }
                }
            }

            AxisSelector = 0;
            if (v[1] > v[0]) AxisSelector = 1;
            if (v[2] > v[AxisSelector]) AxisSelector = 2;

            return candidates;
        }

        private float SortAxis(Vector3 v)
        {
            if (AxisSelector == 0)
                return v.X;
            else if (AxisSelector == 1)
                return v.Y;
            else
                return v.Z;
        }

        bool OverlapBounds(ISweepableBounds a, ISweepableBounds b)
        {
            var distance = a.Center - b.Center;

            var radii = a.Radius + b.Radius;

            return distance.Length() < radii;
        }

        // TODO: current list is provided in no particular order, if we can persist this ordering between frames, we can get better perf here
        // Insertion sort of bodies by Bounds, best for nearly-sorted inputs
        private void InPlaceBodySort(IList<IBody> bodies)
        {
            for(var i = 1; i < bodies.Count; i++)
            {
                var key = bodies[i];
                var j = i - 1;

                while(j >= 0 && SortAxis(bodies[j].Bounds.Min) > SortAxis(key.Bounds.Min))
                {
                    bodies[j + 1] = bodies[j];
                    j--;
                }

                bodies[j + 1] = key;
            }
        }
    }
}
