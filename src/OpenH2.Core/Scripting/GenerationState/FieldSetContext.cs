using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenH2.Core.Tags.Scenario;
using System;
using System.Diagnostics;

namespace OpenH2.Core.Scripting.GenerationState
{
    public class FieldSetContext : BaseGenerationContext, IGenerationContext
    {
        private ExpressionSyntax field = null;
        private ExpressionSyntax value = null;

        public override bool CreatesScope => true;

        public FieldSetContext(ScenarioTag.ScriptSyntaxNode node) : base(node)
        {
        }

        public IGenerationContext AddExpression(ExpressionSyntax expression)
        {
            if(field == null)
            {
                field = expression;
            }
            else if(value == null)
            {
                value = expression;
            }
            else
            {
                throw new Exception("Too many expression provided to FieldSetData");
            }

            return this;
        }

        public void GenerateInto(Scope scope)
        {
            Debug.Assert(field != null, "Field was not provided");
            Debug.Assert(value != null, "Field value was not provided");

            var assignment = SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression, field, value);

            if (scope.IsInStatementContext)
            {
                scope.StatementContext.AddStatement(SyntaxFactory.ExpressionStatement(assignment));
            }
            else
            {
                scope.Context.AddExpression(assignment);
            }
        }
    }
}
