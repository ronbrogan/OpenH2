using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Diagnostics;

namespace OpenH2.ScriptAnalysis.GenerationState
{
    public class UnaryOperatorData : BaseGenerationState, IScriptGenerationState
    {
        private ExpressionSyntax operand;
        private readonly SyntaxKind operatorSyntaxKind;

        public UnaryOperatorData(SyntaxKind operatorSyntaxKind)
        {
            this.operatorSyntaxKind = operatorSyntaxKind;
        }

        public ExpressionSyntax GenerateOperatorExpression()
        {
            Debug.Assert(operand != null, "Not enough operands for unary expression");

            return SyntaxFactory.PrefixUnaryExpression(operatorSyntaxKind, operand);
        }

        public UnaryOperatorData AddOperand(ExpressionSyntax expression)
        {
            Debug.Assert(operand == null, "Can't have multiple operands for a unary expression");

            operand = expression ?? throw new ArgumentNullException(nameof(expression));
            return this;
        }

        public IScriptGenerationState AddExpression(ExpressionSyntax expression) => AddOperand(expression);
    }
}
