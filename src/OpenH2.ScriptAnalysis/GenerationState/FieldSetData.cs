using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Diagnostics;

namespace OpenH2.ScriptAnalysis.GenerationState
{
    public class FieldSetData : BaseGenerationState, IScriptGenerationState, IHoistableGenerationState
    {
        private ExpressionSyntax field = null;
        private ExpressionSyntax value = null;

        public IScriptGenerationState AddExpression(ExpressionSyntax expression)
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

        public StatementSyntax[] GenerateHoistedStatements(out ExpressionSyntax hoistedAccessor)
        {
            hoistedAccessor = field;

            return new StatementSyntax[] { GenerateSetStatement() };
        }

        public StatementSyntax[] GenerateNonHoistedStatements()
        {
            return new StatementSyntax[] { GenerateSetStatement() };
        }

        public StatementSyntax GenerateSetStatement()
        {
            Debug.Assert(field != null, "Field was not provided");
            Debug.Assert(value != null, "Field value was not provided");

            return SyntaxFactory.ExpressionStatement(
                SyntaxFactory.AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression, field, value));
        }
    }
}
