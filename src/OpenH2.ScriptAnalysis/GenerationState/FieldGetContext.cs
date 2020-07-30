using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenH2.Core.Tags.Scenario;
using System;

namespace OpenH2.ScriptAnalysis.GenerationState
{
    public class FieldGetContext : BaseGenerationContext, IExpressionContext
    {
        private readonly ExpressionSyntax accessor;

        public FieldGetContext(ScenarioTag scenario, ScenarioTag.ScriptSyntaxNode node)
        {
            if (node.NodeString == 0)
            {
                accessor = SyntaxFactory.DefaultExpression(SyntaxUtil.ScriptTypeSyntax(node.DataType));
            }
            else
            {
                accessor = SyntaxFactory.IdentifierName(
                    SyntaxUtil.SanitizeIdentifier(
                        SyntaxUtil.GetScriptString(scenario, node)));
            }
        }

        public IExpressionContext AddExpression(ExpressionSyntax expression)
        {
            throw new NotImplementedException();
        }

        public void GenerateInto(Scope scope)
        {
            scope.Context.AddExpression(accessor);
        }
    }
}
