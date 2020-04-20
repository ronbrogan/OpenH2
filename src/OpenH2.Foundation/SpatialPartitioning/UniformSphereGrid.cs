using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace OpenH2.Foundation.SpatialPartitioning
{
    /// <summary>
    /// Uniform grid of spheres where r = 1/2 hypotenuse of cell cube, thus all cube verts lie on the sphere surface
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class UniformSphereGrid<T>
    {
        private IReadOnlyList<T>[,,] cells;

        public UniformSphereGrid(Vector3 min, Vector3 max, float cellSideSize)
        {
            var size = max - min;

            // Calculate how many cells to create on each axis
            // Using ceiling means that the last cells will fall outside of max, but makes checks easier than scaling cells
            var cellCount = size / new Vector3(cellSideSize);
            var xCells = (int)Math.Ceiling(cellCount.X);
            var yCells = (int)Math.Ceiling(cellCount.Y);
            var zCells = (int)Math.Ceiling(cellCount.Z);

            cells = new List<T>[xCells, yCells, zCells];

        }

        public void Insert()
        {

        }

        public IReadOnlyList<T> GetElements(Vector3 center, float radius)
        {
            var indices = GetIndices(center, radius);

            if(indices.Length == 0)
            {
                return Array.Empty<T>();
            }
            else if (indices.Length == 1)
            {
                var (x, y, z) = indices[0];
                return cells[x, y, z];
            }
            else
            {
                var total = 0;
                var cellLists = new IReadOnlyList<T>[8];
                var extraCellListCount = indices.Length - 8;
                var extraCellLists = extraCellListCount > 0
                    ? new IReadOnlyList<T>[extraCellListCount]
                    : Array.Empty<IReadOnlyList<T>>();

                for (int i = 0; i < indices.Length; i++)
                {
                    var (x, y, z) = indices[i];
                    var cell = cells[x, y, z];
                    total += cell.Count;

                    if (i < cellLists.Length)
                    {
                        cellLists[i] = cell;
                    }
                    else
                    {
                        extraCellLists[i] = cell;
                    }
                }

                return new ListUnion(
                    total,
                    cellLists[0],
                    cellLists[1],
                    cellLists[2],
                    cellLists[3],
                    cellLists[4],
                    cellLists[5],
                    cellLists[6],
                    cellLists[7],
                    extraCellLists);
            }
        }

        // Find all cells that bounding sphere overlaps with, can return none, any, or all cells
        private Span<(int, int, int)> GetIndices(Vector3 center, float radius)
        {
            return default;
        }


        /// <summary>
        /// Struct that holds pointers to an arbitrary amount of lists to allow a union list without copying or even allocating in most cases
        /// Probably premature optimization or totally garbage
        /// </summary>
        private struct ListUnion : IReadOnlyList<T>
        {
            private readonly IReadOnlyList<T> list0;
            private readonly IReadOnlyList<T> list1;
            private readonly IReadOnlyList<T> list2;
            private readonly IReadOnlyList<T> list3;
            private readonly IReadOnlyList<T> list4;
            private readonly IReadOnlyList<T> list5;
            private readonly IReadOnlyList<T> list6;
            private readonly IReadOnlyList<T> list7;
            private readonly IReadOnlyList<T>[] extras;

            public T this[int index]
            {
                get
                {
                    var finalIndex = index;

                    if (finalIndex < list0.Count) return list0[finalIndex];
                    else finalIndex -= list0.Count;

                    if (finalIndex < list1.Count) return list1[finalIndex];
                    else finalIndex -= list1.Count;

                    if (finalIndex < list2.Count) return list2[finalIndex];
                    else finalIndex -= list2.Count;

                    if (finalIndex < list3.Count) return list3[finalIndex];
                    else finalIndex -= list3.Count;

                    if (finalIndex < list4.Count) return list4[finalIndex];
                    else finalIndex -= list4.Count;

                    if (finalIndex < list5.Count) return list5[finalIndex];
                    else finalIndex -= list5.Count;

                    if (finalIndex < list6.Count) return list6[finalIndex];
                    else finalIndex -= list6.Count;

                    if (finalIndex < list7.Count) return list7[finalIndex];
                    else finalIndex -= list7.Count;

                    foreach(var extra in extras)
                    {
                        if (finalIndex < extra.Count) return extra[finalIndex];
                        else finalIndex -= extra.Count;
                    }

                    throw new IndexOutOfRangeException();
                }
            }

            public int Count { get; private set; }

            public ListUnion(
                int total,
                IReadOnlyList<T> list0,
                IReadOnlyList<T> list1 = null,
                IReadOnlyList<T> list2 = null,
                IReadOnlyList<T> list3 = null,
                IReadOnlyList<T> list4 = null,
                IReadOnlyList<T> list5 = null,
                IReadOnlyList<T> list6 = null,
                IReadOnlyList<T> list7 = null,
                IReadOnlyList<T>[] extras = null)
            {
                this.Count = total;
                this.list0 = list0 ?? Array.Empty<T>();
                this.list1 = list1 ?? Array.Empty<T>();
                this.list2 = list2 ?? Array.Empty<T>();
                this.list3 = list3 ?? Array.Empty<T>();
                this.list4 = list4 ?? Array.Empty<T>();
                this.list5 = list5 ?? Array.Empty<T>();
                this.list6 = list6 ?? Array.Empty<T>();
                this.list7 = list7 ?? Array.Empty<T>();
                this.extras = extras ?? Array.Empty<IReadOnlyList<T>>();
            }

            public IEnumerator<T> GetEnumerator()
            {
                foreach (var i0 in list0) yield return i0;
                foreach (var i1 in list1) yield return i1;
                foreach (var i2 in list2) yield return i2;
                foreach (var i3 in list3) yield return i3;
                foreach (var i4 in list4) yield return i4;
                foreach (var i5 in list5) yield return i5;
                foreach (var i6 in list6) yield return i6;
                foreach (var i7 in list7) yield return i7;

                foreach (var extra in extras)
                {
                    foreach (var i in extra) yield return i;
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }
        }
    }
}
