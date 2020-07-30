namespace OpenH2.Core.Scripting
{
    public enum ScriptDataType : ushort
    {
        MethodOrOperator = 2,
        Void = 4,
        Boolean = 5,
        Float = 6,
        Short = 7,
        Int = 8,
        String = 9,
        ScriptReference = 10,
        StringId = 11,
        VehicleSeat = 12, // unsure
        Trigger = 13,
        LocationFlag = 14,
        CameraPathTarget = 15,
        CinematicTitle = 16, // unsure
        DeviceGroup = 18,
        AI = 19,
        AIScript = 21,
        AIBehavior = 22,
        AIOrders = 23,
        Equipment = 24,
        Bsp = 26,
        NavigationPoint = 27,
        SpatialPoint = 28,
        List = 31,
        ReferenceGet = 32,
        Effect = 33, // unsure
        Damage = 34,
        LoopingSound = 35,
        Weapon = 38,
        Animation = 36,
        Model = 41,
        GameDifficulty = 44,
        Team = 45,
        DamageState = 48,
        Entity = 50,
        Unit = 51,
        Vehicle = 52,
        WeaponReference = 53,
        Device = 54,
        Scenery = 55,
        EntityIdentifier = 56,
    }

    public enum NodeType : ushort
    {
        Scope = 8,
        Expression = 9,
        ScriptInvocation = 10,
        VariableAccess = 13
    }
}
