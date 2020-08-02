using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenH2.ScriptAnalysis.GenerationState
{
    public interface IStatementContext : IGenerationContext
    {
        IStatementContext AddStatement(StatementSyntax statement);
        bool TryCreateResultStatement(ExpressionSyntax resultValue, out StatementSyntax statement);
    }
}
