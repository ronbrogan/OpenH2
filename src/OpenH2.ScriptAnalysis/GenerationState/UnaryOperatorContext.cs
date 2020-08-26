using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenH2.Core.Scripting;
using OpenH2.Core.Tags.Scenario;
using System;
using System.Diagnostics;

namespace OpenH2.ScriptAnalysis.GenerationState
{
    public class UnaryOperatorContext : BaseGenerationContext, IGenerationContext
    {
        private ExpressionSyntax operand;
        private readonly SyntaxKind operatorSyntaxKind;

        public override bool CreatesScope => true;
        public override ScriptDataType? OwnDataType { get; }

        public UnaryOperatorContext(ScenarioTag.ScriptSyntaxNode node, SyntaxKind operatorSyntaxKind) : base(node)
        {
            this.operatorSyntaxKind = operatorSyntaxKind;
            this.OwnDataType = node.DataType;
        }

        public UnaryOperatorContext AddOperand(ExpressionSyntax expression)
        {
            Debug.Assert(operand == null, "Can't have multiple operands for a unary expression");

            operand = expression ?? throw new ArgumentNullException(nameof(expression));
            return this;
        }

        public IGenerationContext AddExpression(ExpressionSyntax expression) => AddOperand(expression);

        public void GenerateInto(Scope scope)
        {
            Debug.Assert(operand != null, "Not enough operands for unary expression");

            scope.Context.AddExpression(
                SyntaxFactory.PrefixUnaryExpression(
                    operatorSyntaxKind, 
                    SyntaxFactory.ParenthesizedExpression(operand))
                .WithAdditionalAnnotations(ScriptGenAnnotations.TypeAnnotation(this.OwnDataType.Value)));
        }
    }
}
