using System;
using System.Collections.Generic;
using System.Text;

namespace OpenH2.Foundation
{
    public enum EmissiveType
    {
        EmissiveOnly = 0,
        DiffuseBlended = 1,
        ThreeChannel = 2,

        Disabled = -1
    }
}
