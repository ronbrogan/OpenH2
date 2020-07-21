using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace OpenH2.ScriptAnalysis.GenerationState
{
    public class ScopeBlockData : IScriptGenerationState
    {
        public readonly List<StatementSyntax> Statements = new List<StatementSyntax>();

        public ScopeBlockData()
        {

        }

        public IScriptGenerationState AddExpression(ExpressionSyntax expression)
        {
            this.AddStatement(SyntaxFactory.ExpressionStatement(expression));
            return this;
        }

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
