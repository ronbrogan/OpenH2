using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenH2.Core.Scripting.Generation;
using OpenH2.Core.Tags.Scenario;
using System;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace OpenH2.Core.Scripting.GenerationState
{
    public class TagGetContext : BaseGenerationContext, IGenerationContext
    {
        private readonly InvocationExpressionSyntax invocation;
        public override ScriptDataType? OwnDataType { get; }

        public TagGetContext(ScenarioTag scenario, ScenarioTag.ScriptSyntaxNode node) : base(node)
        {
            this.OwnDataType = node.DataType;

            invocation = InvocationExpression(GenericName(Identifier(nameof(IScriptEngine.GetTag)))
                .WithTypeArgumentList(TypeArgumentList(SingletonSeparatedList(
                    SyntaxUtil.ScriptTypeSyntax(this.OwnDataType.Value)))))
                .AddArgumentListArguments(
                    Argument(SyntaxUtil.LiteralExpression(SyntaxUtil.GetScriptString(scenario, node))),
                    Argument(SyntaxUtil.LiteralExpression(node.NodeData_32)))
                .WithAdditionalAnnotations(ScriptGenAnnotations.TypeAnnotation(node.DataType));
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
