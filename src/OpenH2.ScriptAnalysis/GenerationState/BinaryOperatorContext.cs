using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenH2.Core.Scripting;
using OpenH2.Core.Tags.Scenario;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace OpenH2.ScriptAnalysis.GenerationState
{
    public class BinaryOperatorContext : BaseGenerationContext, IGenerationContext
    {
        private readonly List<ExpressionSyntax> operands = new List<ExpressionSyntax>();
        private readonly SyntaxKind operatorSyntaxKind;

        public override bool CreatesScope => true;
        public override ScriptDataType? OwnDataType { get; }

        public BinaryOperatorContext(ScenarioTag.ScriptSyntaxNode node, SyntaxKind operatorSyntaxKind, ScriptDataType returnType) : base(node)
        {
            this.operatorSyntaxKind = operatorSyntaxKind;
            this.OwnDataType = returnType;
        }

        public void GenerateInto(Scope scope)
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

            scope.Context.AddExpression(left);
        }

        public BinaryOperatorContext AddOperand(ExpressionSyntax expression)
        {
            operands.Add(expression);
            return this;
        }

        public IGenerationContext AddExpression(ExpressionSyntax expression) => AddOperand(expression);
    }
}
