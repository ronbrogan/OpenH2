using System;

namespace OpenH2.Physics.Core
{
    [Flags]
    public enum ContactCallbackData
    {
        None = 0,
        TargetVelocity  = 1 << 0,
        Normal          = 1 << 1,
        Point           = 1 << 2,
        Faces           = 1 << 3,
        DynamicFriction = 1 << 4,
        Material = 1 << 5,
    }
}
