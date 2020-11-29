using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace OpenH2.Core.Animation
{
    public struct AnimationFrame
    {
        public Quaternion Orientation { get; set; }
        public Vector3 Translation { get; set; }

        public AnimationFrame(Quaternion orient, Vector3 translate)
        {
            this.Orientation = orient;
            this.Translation = translate;
        }
    }
}
