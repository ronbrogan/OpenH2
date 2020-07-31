using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenH2.Core.Scripting;
using OpenH2.Core.Tags.Scenario;
using System;

namespace OpenH2.ScriptAnalysis.GenerationState
{
    public class LiteralContext : BaseGenerationContext, IGenerationContext
    {
        private LiteralExpressionSyntax literal;
        public override ScriptDataType? OwnDataType { get; }

        public LiteralContext(ScenarioTag scenario, ScenarioTag.ScriptSyntaxNode node) : base(node)
        {
            this.literal = SyntaxUtil.LiteralExpression(scenario, node);
            this.OwnDataType = node.DataType;
        }

        public IGenerationContext AddExpression(ExpressionSyntax expression)
        {
            throw new NotImplementedException();
        }

        public void GenerateInto(Scope scope)
        {
            scope.Context.AddExpression(literal);
        }
    }
}
