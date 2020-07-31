using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenH2.Core.Scripting;
using OpenH2.Core.Tags.Scenario;
using System;
using System.Collections.Generic;

namespace OpenH2.ScriptAnalysis.GenerationState
{
    public class BeginCallContext : BaseGenerationContext, IGenerationContext, IStatementContext
    {
        private readonly Scope context;
        private readonly ScriptDataType returnType;

        public override bool CreatesScope => true;

        public List<StatementSyntax> Body { get; } = new List<StatementSyntax>();

        public BeginCallContext(ScenarioTag.ScriptSyntaxNode node, Scope context, ScriptDataType returnType) : base(node)
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
                Body.Add(resultGen(lastExp));
            }
            else if (last.TryGetRightHandExpression(out var rhsExp))
            {
                Body.Add(resultGen(rhsExp));
            }
            else
            {
                Body.Add(resultGen(
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

            Body.Add(SyntaxFactory.ExpressionStatement(exp));

            return this;
        }

        IGenerationContext IGenerationContext.AddExpression(ExpressionSyntax expression) => AddExpression(expression);

        public IStatementContext AddStatement(StatementSyntax statement)
        {
            if (Body.Count == 0)
            {
                // TODO: This is the last expression of the body, ensure result is stored/passed first
                //EnsureReturnStatement(statement);
            }
            //else { Body.Push(statement); }

            Body.Add(statement);
            return this;
        }

        public StatementSyntax CreateResultStatement(ExpressionSyntax resultValue)
        {
            return SyntaxFactory.ReturnStatement(resultValue)
                .WithAdditionalAnnotations(ScriptGenAnnotations.ResultStatement);
        }
    }
}
