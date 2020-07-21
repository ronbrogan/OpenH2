using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace OpenH2.ScriptAnalysis.GenerationState
{
    public interface IScriptGenerationState
    {
        IScriptGenerationState AddExpression(ExpressionSyntax expression);
    }
}