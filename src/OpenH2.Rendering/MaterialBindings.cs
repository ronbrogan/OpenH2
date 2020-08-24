using System;
using System.Collections.Generic;
using System.Text;

namespace OpenH2.Rendering
{
    public struct MaterialBindings
    {
        public long DiffuseHandle { get; set; }
        public long AlphaHandle { get; set; }
        public long SpecularHandle { get; set; }
        public long EmissiveHandle { get; set; }
        public long NormalHandle { get; set; }
        public long Detail1Handle { get; set; }
        public long Detail2Handle { get; set; }
    }
}
