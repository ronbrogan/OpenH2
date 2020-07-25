using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace OpenH2.ScriptAnalysis.GenerationState
{
    public class ScopeBlockData : IScriptGenerationState, IScopedScriptGenerationState
    {
        public List<StatementSyntax> Statements { get; set; } = new List<StatementSyntax>();

        public ScopeBlockData()
        {

        }

        public IScriptGenerationState AddExpression(ExpressionSyntax expression)
        {
            this.AddStatement(SyntaxFactory.ExpressionStatement(expression));
            return this;
        }

        IScopedScriptGenerationState IScopedScriptGenerationState.AddStatement(StatementSyntax statement) => AddStatement(statement);
        public ScopeBlockData AddStatement(StatementSyntax statement)
        {
            this.Statements.Add(statement);
            return this;
        }

        public BlockSyntax GenerateBlock()
        {
            return SyntaxFactory.Block(SyntaxFactory.List(Statements));
        }
    }
}
