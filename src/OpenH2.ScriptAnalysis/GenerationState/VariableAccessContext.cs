using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenH2.Core.Tags.Scenario;
using System;

namespace OpenH2.ScriptAnalysis.GenerationState
{
    public class VariableAccessContext : BaseGenerationContext, IExpressionContext
    {
        private readonly MemberAccessExpressionSyntax access;

        public VariableAccessContext(ScenarioTag tag, ScenarioTag.ScriptSyntaxNode node)
        {
            access = SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                SyntaxFactory.ThisExpression(),
                SyntaxFactory.IdentifierName(
                    SyntaxUtil.SanitizeIdentifier(
                        SyntaxUtil.GetScriptString(tag, node))));
        }

        public IExpressionContext AddExpression(ExpressionSyntax expression)
        {
            throw new NotImplementedException();
        }

        public void GenerateInto(Scope scope)
        {
            scope.Context.AddExpression(access);
        }
    }
}
