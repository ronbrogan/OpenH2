using System;
using System.Collections.Generic;
using System.Text;

namespace OpenH2.Core.Types
{
    public class Triangle
    {
        public (int, int, int) Indicies { get; set; }

        public short MaterialId { get; set; }
    }
}
