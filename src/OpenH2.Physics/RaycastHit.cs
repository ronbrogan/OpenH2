using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace OpenH2.Physics
{
    public class RaycastHit
    {
        public float Distance { get; }
        public Vector3 Position { get; }
        public Vector3 Normal { get; }

        public RaycastHit(float distance, Vector3 pos, Vector3 normal)
        {
            this.Distance = distance;
            this.Position = pos;
            this.Normal = normal;
        }
    }
}
