using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenH2.Core.Scripting.Generation;
using OpenH2.Core.Tags.Scenario;
using System;

namespace OpenH2.Core.Scripting.GenerationState
{
    public class UnknownContext : BaseGenerationContext, IGenerationContext
    {
        private readonly InvocationExpressionSyntax invocation;
        public override ScriptDataType? OwnDataType { get; }

        public UnknownContext(ScenarioTag scenario, ScenarioTag.ScriptSyntaxNode node) : base(node)
        {
            this.OwnDataType = node.DataType;

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

        public IGenerationContext AddExpression(ExpressionSyntax expression)
        {
            throw new NotImplementedException();
        }

        public void GenerateInto(Scope scope)
        {
            scope.Context.AddExpression(this.invocation);
        }
    }
}
