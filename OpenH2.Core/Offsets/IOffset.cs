using System;
using System.Collections.Generic;
using System.Text;

namespace OpenH2.Core.Offsets
{
    interface IOffset
    {
        int Value { get; }
        int OriginalValue { get; }
    }
}
