using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace OpenH2.ScriptAnalysis.GenerationState
{
    public interface IScriptGenerationState
    {
        IReadOnlyList<object> Metadata { get; }

        IScriptGenerationState AddExpression(ExpressionSyntax expression);

        void AddMetadata(object metadata);
    }
}