using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace OpenH2.Core.Scripting.GenerationState
{
    public interface IStatementContext : IGenerationContext
    {
        IStatementContext AddStatement(StatementSyntax statement);
        bool TryCreateResultStatement(ExpressionSyntax resultValue, out StatementSyntax statement);
    }
}
