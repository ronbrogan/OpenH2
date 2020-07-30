using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenH2.Core.Scripting;
using System;
using System.Collections.Generic;

namespace OpenH2.ScriptAnalysis.GenerationState
{
    public class BeginCallContext : BaseGenerationContext, IExpressionContext, IStatementContext
    {
        private readonly Scope context;
        private readonly ScriptDataType returnType;

        public Stack<StatementSyntax> Body { get; } = new Stack<StatementSyntax>();

        public BeginCallContext(Scope context, ScriptDataType returnType)
        {
            this.context = context;
            this.returnType = returnType;
        }

        public void GenerateInto(Scope scope)
        {
            if(scope.IsInStatementContext)
            {
                foreach (var b in Body)
                {
                    scope.StatementContext.AddStatement(b);
                }

                return;
            }
            else
            {
                scope.Context.AddExpression(
                    SyntaxUtil.CreateImmediatelyInvokedFunction(returnType, Body));
            }
        }

        private void EnsureReturnStatement(StatementSyntax last, Func<ExpressionSyntax, StatementSyntax> resultGen)
        {
            if (last.TryGetContainingSimpleExpression(out var lastExp))
            {
                Body.Push(resultGen(lastExp));
            }
            else if (last.TryGetRightHandExpression(out var rhsExp))
            {
                Body.Push(resultGen(rhsExp));
            }
            else
            {
                Body.Push(resultGen(
                    SyntaxFactory.LiteralExpression(
                        SyntaxKind.DefaultLiteralExpression,
                        SyntaxFactory.Token(SyntaxKind.DefaultKeyword)))
                    .WithTrailingTrivia(SyntaxFactory.Comment("// Unhandled 'begin' return")));
            }
        }

        public BeginCallContext AddExpression(ExpressionSyntax exp)
        {
            if(Body.Count == 0)
            {
                // TODO: This is the last expression of the body, ensure result is stored/passed first
                //EnsureReturnStatement(exp);
            }
            //else { Body.Push(statement); }

            Body.Push(SyntaxFactory.ExpressionStatement(exp));

            return this;
        }

        IExpressionContext IExpressionContext.AddExpression(ExpressionSyntax expression) => AddExpression(expression);

        public IStatementContext AddStatement(StatementSyntax statement)
        {
            if (Body.Count == 0)
            {
                // TODO: This is the last expression of the body, ensure result is stored/passed first
                //EnsureReturnStatement(statement);
            }
            //else { Body.Push(statement); }

            Body.Push(statement);
            return this;
        }

        public StatementSyntax CreateResultStatement(ExpressionSyntax resultValue)
        {
            return SyntaxFactory.ReturnStatement(resultValue)
                .WithAdditionalAnnotations(ScriptGenAnnotations.ResultStatement);
        }

        public StatementSyntax[] GetInnerStatements()
        {
            return this.Body.ToArray();
        }
    }
}
