using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Diagnostics;

namespace OpenH2.ScriptAnalysis.GenerationState
{
    public class FieldSetContext : BaseGenerationContext, IExpressionContext
    {
        private ExpressionSyntax field = null;
        private ExpressionSyntax value = null;

        public IExpressionContext AddExpression(ExpressionSyntax expression)
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

        //public StatementSyntax[] GenerateHoistedStatements(out ExpressionSyntax hoistedAccessor)
        //{
        //    hoistedAccessor = field;

        //    return new StatementSyntax[] { GenerateSetStatement() };
        //}

        //public StatementSyntax[] GenerateNonHoistedStatements()
        //{
        //    return new StatementSyntax[] { GenerateSetStatement() };
        //}

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
