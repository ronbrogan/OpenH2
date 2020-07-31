using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenH2.Core.Scripting;
using OpenH2.Core.Tags.Scenario;
using System;

namespace OpenH2.ScriptAnalysis.GenerationState
{
    public class ReferenceGetContext : BaseGenerationContext, IGenerationContext
    {
        private readonly InvocationExpressionSyntax invocation;
        public override ScriptDataType? OwnDataType { get; }

        public ReferenceGetContext(ScenarioTag scenario, ScenarioTag.ScriptSyntaxNode node) : base(node)
        {
            this.OwnDataType = node.DataType;

            invocation = SyntaxFactory.InvocationExpression(
                SyntaxFactory.IdentifierName("GetReference"),
                SyntaxFactory.ArgumentList(
                    SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.Argument(
                            SyntaxUtil.LiteralExpression(
                                SyntaxUtil.GetScriptString(scenario, node))))));
        }

        public IGenerationContext AddExpression(ExpressionSyntax expression)
        {
            throw new NotImplementedException();
        }

        public void GenerateInto(Scope scope)
        {
            scope.Context.AddExpression(invocation);
        }
    }
}
