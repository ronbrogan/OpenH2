using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace OpenH2.ScriptAnalysis.GenerationState
{
    public class BinaryOperatorData : BaseGenerationState, IScriptGenerationState
    {
        private readonly List<ExpressionSyntax> operands = new List<ExpressionSyntax>();
        private readonly SyntaxKind operatorSyntaxKind;

        public BinaryOperatorData(SyntaxKind operatorSyntaxKind)
        {
            this.operatorSyntaxKind = operatorSyntaxKind;
        }

        public ExpressionSyntax GenerateOperatorExpression()
        {
            Debug.Assert(operands.Count >= 2, "Not enough operands for binary expression");

            var ops = new Queue<ExpressionSyntax>();

            foreach(var op in operands)
            {
                ops.Enqueue(op);
            }

            var left = ops.Dequeue();

            while(ops.Any())
            {
                left = SyntaxFactory.BinaryExpression(operatorSyntaxKind,
                    left, ops.Dequeue());
            }

            return left;
        }

        public BinaryOperatorData AddOperand(ExpressionSyntax expression)
        {
            operands.Add(expression);
            return this;
        }

        public IScriptGenerationState AddExpression(ExpressionSyntax expression) => AddOperand(expression);
    }
}
