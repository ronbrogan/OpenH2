using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenH2.ScriptAnalysis.GenerationState
{
    public interface IStatementContext
    {
        IStatementContext AddStatement(StatementSyntax statement);
        StatementSyntax CreateResultStatement(ExpressionSyntax resultValue);
        StatementSyntax[] GetInnerStatements();
    }
}
