using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace OpenH2.Foundation
{
    public class Model
    {
        public Mesh[] Meshes { get; set; }

        public Matrix4x4 Transform { get; set; } 
    }
}
