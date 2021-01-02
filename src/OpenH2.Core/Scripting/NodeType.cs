namespace OpenH2.Core.Scripting
{

    public enum NodeType : ushort
    {
        BuiltinInvocation = 8,
        Expression = 9,
        ScriptInvocation = 10,
        VariableAccess = 13,

        MethodDecl = ushort.MaxValue
    }
}
