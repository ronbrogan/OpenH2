using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;
using OpenH2.Core.Scripting;
using OpenH2.Core.Tags.Scenario;
using System;
using Microsoft.CodeAnalysis;

namespace OpenH2.ScriptAnalysis.GenerationState
{
    public class ReferenceGetContext : BaseGenerationContext, IGenerationContext
    {
        private readonly InvocationExpressionSyntax invocation;
        public override ScriptDataType? OwnDataType { get; }

        public ReferenceGetContext(ScenarioTag scenario, ScenarioTag.ScriptSyntaxNode node) : base(node)
        {
            this.OwnDataType = node.DataType;

            invocation = InvocationExpression(GenericName(Identifier("GetReference"))
                .WithTypeArgumentList(TypeArgumentList(SingletonSeparatedList(
                    SyntaxUtil.ScriptTypeSyntax(this.OwnDataType.Value)))),
                ArgumentList(
                    SingletonSeparatedList(
                        Argument(
                            SyntaxUtil.LiteralExpression(
                                SyntaxUtil.GetScriptString(scenario, node))))))
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
