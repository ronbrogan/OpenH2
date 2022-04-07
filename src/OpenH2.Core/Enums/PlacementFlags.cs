using System;

namespace OpenH2.Core.Enums
{
    [Flags]
    public enum PlacementFlags : ushort
    {
        NotAutomatically        = 1 << 0,
        Unused1                 = 1 << 1,
        Unused2                 = 1 << 2,
        Unused3                 = 1 << 3,
        LockTypeToEnvObject     = 1 << 4,
        LockTransformToEnvObject= 1 << 5,
        NeverPlaced             = 1 << 6,
        LockNameToEnvObject     = 1 << 7,
        CreateAtRest            = 1 << 8,
    }
}
