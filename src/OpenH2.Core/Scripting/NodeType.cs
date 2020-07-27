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
        Emotion = 11,
        Trigger = 13,
        LocationFlag = 14,
        DeviceGroup = 18,
        AI = 19,
        AIScript = 21,
        AIOrders = 23,
        Bsp = 26,
        List = 31,
        ReferenceGet = 32,
        Effect = 33, // unsure
        LoopingSound = 35,
        Weapon = 38,
        Animation = 36,
        GameDifficulty = 44,
        Entity = 50,
        Unit = 51,
        Device = 54,
        Scenery = 55,
        EntityIdentifier = 56,
    }

    public enum NodeType : ushort
    {
        ExpressionScope = 8,
        Statement = 9,
        ScriptInvocation = 10,
        VariableAccess = 13
    }
}
