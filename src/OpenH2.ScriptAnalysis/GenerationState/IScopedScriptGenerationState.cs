using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenH2.ScriptAnalysis.GenerationState
{
    public interface IScopedScriptGenerationState
    {
        IScopedScriptGenerationState AddStatement(StatementSyntax statement);
        StatementSyntax CreateResultStatement(ExpressionSyntax resultValue);
    }
}
