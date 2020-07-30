﻿using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Diagnostics;

namespace OpenH2.ScriptAnalysis.GenerationState
{
    public class SingleExpressionStatementContext : BaseGenerationContext, IExpressionContext, IStatementContext
    {
        private ExpressionSyntax expression;

        public IExpressionContext AddExpression(ExpressionSyntax expression)
        {
            Debug.Assert(this.expression == null, $"More than one expression added to {nameof(SingleExpressionStatementContext)}");

            this.expression = expression;
            return this;
        }

        public ExpressionSyntax GetInnerExpression()
        {
            return this.expression;
        }

        public void GenerateInto(Scope scope)
        {
            scope.Context.AddExpression(this.expression);
        }

        public IStatementContext AddStatement(StatementSyntax statement)
        {
            var exp = statement as ExpressionStatementSyntax;
            Debug.Assert(exp != null, $"Can't add arbitrary statements into {nameof(SingleExpressionStatementContext)}");

            Debug.Assert(this.expression == null, $"More than one expression added to {nameof(SingleExpressionStatementContext)}");

            this.expression = exp.Expression;
            return this;
        }

        public StatementSyntax CreateResultStatement(ExpressionSyntax resultValue)
        {
            throw new NotImplementedException();
        }

        public StatementSyntax[] GetInnerStatements()
        {
            throw new NotImplementedException();
        }
    }
}
