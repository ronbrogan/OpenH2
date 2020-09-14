using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenH2.Core.Scripting.Generation;
using OpenH2.Core.Tags.Scenario;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace OpenH2.Core.Scripting.GenerationState
{
    public class BinaryOperatorContext : BaseGenerationContext, IGenerationContext
    {
        private readonly List<ExpressionSyntax> operands = new List<ExpressionSyntax>();
        private readonly SyntaxKind operatorSyntaxKind;

        public override bool CreatesScope => true;
        public override ScriptDataType? OwnDataType { get; }

        private HashSet<SyntaxKind> numericPromotionOperators = new HashSet<SyntaxKind>()
        {
            SyntaxKind.AddExpression, SyntaxKind.SubtractExpression, SyntaxKind.MultiplyExpression, SyntaxKind.DivideExpression, SyntaxKind.ModuloExpression
        };

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
            ScriptDataType? topType = this.OwnDataType;

            while(ops.Any())
            {
                var right = ops.Dequeue();

                var binExp = SyntaxFactory.BinaryExpression(operatorSyntaxKind,
                    left, right);

                if (numericPromotionOperators.Contains(this.operatorSyntaxKind) &&
                    SyntaxUtil.TryGetTypeOfExpression(left, out var leftType) &&
                    SyntaxUtil.TryGetTypeOfExpression(right, out var rightType))
                {
                    var promoted = SyntaxUtil.BinaryNumericPromotion(leftType, rightType);
                    binExp = binExp.WithAdditionalAnnotations(ScriptGenAnnotations.TypeAnnotation(promoted));
                    topType = promoted;
                }

                left = binExp;
            }

            if(topType.HasValue && topType.Value != scope.Type)
            {
                left = SyntaxUtil.CreateCast(topType.Value, scope.Type, SyntaxFactory.ParenthesizedExpression(left));
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
