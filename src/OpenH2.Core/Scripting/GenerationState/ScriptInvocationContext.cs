using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenH2.Core.Scripting.Generation;
using OpenH2.Core.Tags.Scenario;
using System;

namespace OpenH2.Core.Scripting.GenerationState
{
    public class ScriptInvocationContext : BaseGenerationContext, IGenerationContext
    {
        private readonly InvocationExpressionSyntax invocation;

        public ScriptInvocationContext(ScenarioTag scenario, ScenarioTag.ScriptSyntaxNode node) : base(node)
        {
            var method = scenario.ScriptMethods[node.OperationId];

            invocation = SyntaxFactory.InvocationExpression(
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.ThisExpression(),
                    SyntaxFactory.IdentifierName(
                        SyntaxUtil.SanitizeIdentifier(method.Description))))
                .WithAdditionalAnnotations(ScriptGenAnnotations.TypeAnnotation(method.ReturnType));
        }

        public IGenerationContext AddExpression(ExpressionSyntax expression)
        {
            throw new NotImplementedException();
        }

        public void GenerateInto(Scope scope)
        {
            scope.Context.AddExpression(SyntaxFactory.AwaitExpression(this.invocation));
        }
    }
}
