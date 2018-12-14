using System;
using System.Collections.Generic;
using System.Text;

namespace OpenH2.Core.Enums
{
    [Flags]
    public enum SceneReadOptions
    {
        Default = 0,
        SkipTagBuilding = 1 << 0
    }
}
