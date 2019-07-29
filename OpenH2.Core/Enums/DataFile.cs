using System;
using System.Collections.Generic;
using System.Text;

namespace OpenH2.Core.Enums
{
    public enum DataFile
    {
        Local = 0b0,
        MainMenu = 0b100,
        Shared = 0b1000,
        SinglePlayerShared = 0b1
    }
}
