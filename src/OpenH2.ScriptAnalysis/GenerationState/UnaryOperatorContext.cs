using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Diagnostics;

namespace OpenH2.ScriptAnalysis.GenerationState
{
    public class UnaryOperatorContext : BaseGenerationContext, IExpressionContext
    {
        private ExpressionSyntax operand;
        private readonly SyntaxKind operatorSyntaxKind;

        public UnaryOperatorContext(SyntaxKind operatorSyntaxKind)
        {
            this.operatorSyntaxKind = operatorSyntaxKind;
        }

        public UnaryOperatorContext AddOperand(ExpressionSyntax expression)
        {
            Debug.Assert(operand == null, "Can't have multiple operands for a unary expression");

            operand = expression ?? throw new ArgumentNullException(nameof(expression));
            return this;
        }

        public IExpressionContext AddExpression(ExpressionSyntax expression) => AddOperand(expression);

        public void GenerateInto(Scope scope)
        {
            Debug.Assert(operand != null, "Not enough operands for unary expression");

            scope.Context.AddExpression(SyntaxFactory.PrefixUnaryExpression(operatorSyntaxKind, operand));
        }
    }
}
