using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenH2.Core.Tags.Scenario;
using System;

namespace OpenH2.ScriptAnalysis.GenerationState
{
    public class ScriptInvocationContext : BaseGenerationContext, IExpressionContext
    {
        private readonly InvocationExpressionSyntax invocation;

        public ScriptInvocationContext(ScenarioTag scenario, ScenarioTag.ScriptSyntaxNode node)
        {
            invocation = SyntaxFactory.InvocationExpression(
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.ThisExpression(),
                    SyntaxFactory.IdentifierName(
                        SyntaxUtil.SanitizeIdentifier(
                            scenario.ScriptMethods[node.ScriptIndex].Description))));
        }

        public IExpressionContext AddExpression(ExpressionSyntax expression)
        {
            throw new NotImplementedException();
        }

        public void GenerateInto(Scope scope)
        {
            scope.Context.AddExpression(this.invocation);
        }
    }
}
