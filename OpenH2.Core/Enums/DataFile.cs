using System;
using System.Collections.Generic;
using System.Text;

namespace OpenH2.Core.Enums
{
    public enum DataFile
    {
        Local = 0b00,
        MainMenu = 0b01,
        Shared = 0b10,
        SinglePlayerShared = 0b11
    }
}
