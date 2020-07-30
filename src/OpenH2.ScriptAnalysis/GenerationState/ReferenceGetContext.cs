using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenH2.Core.Tags.Scenario;
using System;

namespace OpenH2.ScriptAnalysis.GenerationState
{
    public class ReferenceGetContext : BaseGenerationContext, IExpressionContext
    {
        private readonly InvocationExpressionSyntax invocation;

        public ReferenceGetContext(ScenarioTag scenario, ScenarioTag.ScriptSyntaxNode node)
        {
            invocation = SyntaxFactory.InvocationExpression(
                SyntaxFactory.IdentifierName("GetReference"),
                SyntaxFactory.ArgumentList(
                    SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.Argument(
                            SyntaxUtil.LiteralExpression(
                                SyntaxUtil.GetScriptString(scenario, node))))));
        }

        public IExpressionContext AddExpression(ExpressionSyntax expression)
        {
            throw new NotImplementedException();
        }

        public void GenerateInto(Scope scope)
        {
            scope.Context.AddExpression(invocation);
        }
    }
}
