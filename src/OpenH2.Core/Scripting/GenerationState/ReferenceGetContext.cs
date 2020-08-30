using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenH2.Core.Scripting.Generation;
using OpenH2.Core.Tags.Scenario;
using System;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace OpenH2.Core.Scripting.GenerationState
{
    public class ReferenceGetContext : BaseGenerationContext, IGenerationContext
    {
        private readonly InvocationExpressionSyntax invocation;
        public override ScriptDataType? OwnDataType { get; }

        public ReferenceGetContext(ScenarioTag scenario, ScenarioTag.ScriptSyntaxNode node) : base(node)
        {
            this.OwnDataType = node.DataType;

            invocation = InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName("Engine"),
                    GenericName(Identifier("GetReference"))
                .WithTypeArgumentList(TypeArgumentList(SingletonSeparatedList(
                    SyntaxUtil.ScriptTypeSyntax(this.OwnDataType.Value))))),
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
