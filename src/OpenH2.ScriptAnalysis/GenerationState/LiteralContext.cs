using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenH2.Core.Scripting;
using OpenH2.Core.Tags.Scenario;
using System;
using System.Linq;

namespace OpenH2.ScriptAnalysis.GenerationState
{
    public class LiteralContext : BaseGenerationContext, IGenerationContext
    {
        private LiteralExpressionSyntax literal;

        public override ScriptDataType? OwnDataType { get; }

        public LiteralContext(ScenarioTag scenario, ScenarioTag.ScriptSyntaxNode node, Scope containing) : base(node)
        {
            this.OwnDataType = node.DataType;

            var dest = node.DataType;

            if(SyntaxUtil.NumericLiteralTypes.Contains(node.DataType)
                && containing.Context is BinaryOperatorContext bin 
                && SyntaxUtil.NumericLiteralTypes.Contains(bin.OwnDataType.Value))
            {
                dest = bin.OwnDataType.Value;
            }

            this.literal = SyntaxUtil.LiteralExpression(scenario, node, dest);
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
