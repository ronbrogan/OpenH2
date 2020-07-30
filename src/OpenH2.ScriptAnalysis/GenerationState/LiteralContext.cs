using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenH2.Core.Tags.Scenario;
using System;

namespace OpenH2.ScriptAnalysis.GenerationState
{
    public class LiteralContext : BaseGenerationContext, IExpressionContext
    {
        private LiteralExpressionSyntax literal;

        public LiteralContext(ScenarioTag scenario, ScenarioTag.ScriptSyntaxNode node)
        {
            this.literal = SyntaxUtil.LiteralExpression(scenario, node);
        }

        public IExpressionContext AddExpression(ExpressionSyntax expression)
        {
            throw new NotImplementedException();
        }

        public void GenerateInto(Scope scope)
        {
            scope.Context.AddExpression(literal);
        }
    }
}
