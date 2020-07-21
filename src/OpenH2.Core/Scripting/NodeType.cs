namespace OpenH2.Core.Scripting
{
    public enum ScriptDataType : ushort
    {
        MethodOrOperator = 2,
        StatementStart = 4,
        Boolean = 5,
        Float = 6,
        Short = 7,
        Int = 8,
        String = 9,
        Trigger = 13,
        LocationFlag = 14,
        AI = 19,
        AIScript = 21,
        List = 31,
        ReferenceGet = 32,
        Entity = 50,
        Device = 54,
        EntityIdentifier = 56,
    }

    public enum NodeType : ushort
    {
        ExpressionScope = 8,
        Statement = 9,
        VariableAccess = 13
    }
}
