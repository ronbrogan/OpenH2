using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenH2.Core.Scripting;
using OpenH2.Core.Tags.Scenario;
using System;

namespace OpenH2.ScriptAnalysis.GenerationState
{
    public class VariableAccessContext : BaseGenerationContext, IGenerationContext
    {
        private readonly MemberAccessExpressionSyntax access;
        public override ScriptDataType? OwnDataType { get; }

        public VariableAccessContext(ScenarioTag tag, ScenarioTag.ScriptSyntaxNode node) : base(node)
        {
            this.OwnDataType = node.DataType;

            access = SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                SyntaxFactory.ThisExpression(),
                SyntaxFactory.IdentifierName(
                    SyntaxUtil.SanitizeIdentifier(
                        SyntaxUtil.GetScriptString(tag, node))))
                .WithAdditionalAnnotations(ScriptGenAnnotations.TypeAnnotation(node.DataType));
        }

        public IGenerationContext AddExpression(ExpressionSyntax expression)
        {
            throw new NotImplementedException();
        }

        public void GenerateInto(Scope scope)
        {
            scope.Context.AddExpression(access);
        }
    }
}
