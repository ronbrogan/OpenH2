using Microsoft.CodeAnalysis;

namespace OpenH2.Core.Scripting.Generation
{
    public static class ScriptGenAnnotations
    {
        public static SyntaxAnnotation ResultStatement { get; } = new SyntaxAnnotation("ResultStatement");
        public static SyntaxAnnotation FinalScopeStatement { get; } = new SyntaxAnnotation("FinalScopeStatement");
        public static SyntaxAnnotation IfStatement { get; } = new SyntaxAnnotation("IfStatement");
        public static SyntaxAnnotation HoistedResultVar { get; } = new SyntaxAnnotation("HoistedResultVar");

        public const string TypeAnnotationKind = "TypeAnnotation";
        public static SyntaxAnnotation TypeAnnotation(ScriptDataType t) => new SyntaxAnnotation(TypeAnnotationKind, ((int)t).ToString());
    }
}
