using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenH2.Core.Tags.Scenario;
using System;

namespace OpenH2.ScriptAnalysis.GenerationState
{
    public class UnknownContext : BaseGenerationContext, IExpressionContext
    {
        private readonly InvocationExpressionSyntax invocation;

        public UnknownContext(ScenarioTag scenario, ScenarioTag.ScriptSyntaxNode node)
        {
            string unknownDescription = "";

            if (node.NodeString > 0 && node.NodeString < scenario.ScriptStrings.Length
                && scenario.ScriptStrings[node.NodeString - 1] == 0)
            {
                unknownDescription = SyntaxUtil.GetScriptString(scenario, node);
            }

            unknownDescription += $" -{node.NodeType}<{node.DataType}>";
            invocation = SyntaxFactory.InvocationExpression(
                SyntaxFactory.IdentifierName("UNKNOWN"),
                SyntaxFactory.ArgumentList(
                    SyntaxFactory.SingletonSeparatedList(
                        SyntaxFactory.Argument(SyntaxUtil.LiteralExpression(unknownDescription)))));
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
