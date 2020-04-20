using OpenH2.Foundation.Extensions;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace OpenH2.Physics.SpatialPartitioning
{
    /// <summary>
    /// Non-cubic octree, inspired by Ericson 2005 p. 310
    /// Creates children on construction.
    /// Objects get stuck at higher levels when straddling dividing planes.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class OctreeNode<T>
    {
        public Vector3 Min { get; private set; }
        public Vector3 Max { get; private set; }
        public Vector3 Center { get; private set; }

        private OctreeNode<T>[] children = null;
        private List<T> nodeItems = new List<T>(16);

        public OctreeNode(Vector3 Min, Vector3 Max, float minCellSize)
        {
            this.Min = Min;
            this.Max = Max;

            var cellSize = Max - Min;
            this.Center = Min + (cellSize / 2);

            if (cellSize.X > minCellSize && cellSize.Y > minCellSize && cellSize.Z > minCellSize)
            {
                children = new OctreeNode<T>[8];

                var childCellSize = cellSize * 0.5f;
                var halfChildCellSize = childCellSize * 0.5f;

                var childCenter = Vector3.Zero;
                for (var i = 0; i < 8; i++)
                {
                    childCenter.X = ((i & 1) == 1) ? halfChildCellSize.X : -halfChildCellSize.X;
                    childCenter.Y = ((i & 2) == 2) ? halfChildCellSize.Y : -halfChildCellSize.Y;
                    childCenter.Z = ((i & 4) == 4) ? halfChildCellSize.Z : -halfChildCellSize.Z;

                    var childMin = this.Center  + childCenter - halfChildCellSize;
                    var childMax = childMin + childCellSize;

                    children[i] = new OctreeNode<T>(childMin, childMax, minCellSize);
                }
            }
            else
            {
                children = new OctreeNode<T>[8];
                for (var i = 0; i < 8; i++)
                {
                    children[i] = null;
                }
            }
        }

        public void Insert(Vector3 center, Vector3 halfWidths, T item)
        {
            if(TryGetChildIndex(center, halfWidths, out var index) && children[index] != null)
            {
                children[index].Insert(center, halfWidths, item);
            }
            else
            {
                this.nodeItems.Add(item);
            }
        }

        public void GetOverlappingItems(Vector3 center, Vector3 halfWidths, ref List<T> results)
        {
            results.AddRange(this.nodeItems);

            if(TryGetChildIndex(center, halfWidths, out var index))
            {
                // if this fits entirely in a child space, only hit that one
                this.children[index]?.GetOverlappingItems(center, halfWidths, ref results);
            }
            else
            {
                // PERF: only descend into necessary children
                foreach(var child in this.children)
                {
                    child?.GetOverlappingItems(center, halfWidths, ref results);
                }
            }

        }

        // compute octant the volume is in, if stradling any dividing planes, exit
        private bool TryGetChildIndex(Vector3 center, Vector3 halfWidths, out int index)
        {
            index = 0;

            // compute octant the min/max is in, stradling any dividing planes, exit
            for (var i = 0; i < 3; i++)
            {
                var delta = center.Axis(i) - this.Center.Axis(i);
                if (Math.Abs(delta) < halfWidths.Axis(i))
                {
                    return false;
                }

                if (delta > 0f)
                {
                    index |= (1 << i);
                }
            }

            return true;
        }
    }
}
