using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace OpenH2.Physics.Core
{
    public class ContactInfo
    {
        public Vector3 TargetVelocity { get; set; }
        public Vector3 Normal { get; set; }
        public (int, int) Faces { get; set; }
        public Vector3 Point { get; set; }
        public float DynamicFriction { get; set; }
        public bool IsGroundContact { get; set; }
        public object Material { get; set; }
    }
}
