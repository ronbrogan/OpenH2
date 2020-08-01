using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenH2.Core.Scripting;
using OpenH2.Core.Tags.Scenario;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenH2.ScriptAnalysis.GenerationState
{
    public class BeginCallContext : BaseGenerationContext, IGenerationContext, IStatementContext
    {
        private readonly ScriptDataType returnType;

        public override bool CreatesScope => true;

        public List<StatementSyntax> Body { get; } = new List<StatementSyntax>();

        public BeginCallContext(ScenarioTag.ScriptSyntaxNode node, Scope context, ScriptDataType returnType) : base(node)
        {
            this.returnType = returnType;
        }

        public void GenerateInto(Scope scope)
        {
            if(scope.IsInStatementContext)
            {
                var lastExp = Body.Last();
                EnsureReturnStatement(lastExp, scope.StatementContext.CreateResultStatement);

                foreach (var b in Body)
                {
                    scope.StatementContext.AddStatement(b);
                }
            }
            else
            {
                var lastExp = Body.Last();

                EnsureReturnStatement(lastExp, e => SyntaxFactory.ReturnStatement(e)
                    .WithAdditionalAnnotations(ScriptGenAnnotations.ResultStatement));

                scope.Context.AddExpression(
                    SyntaxUtil.CreateImmediatelyInvokedFunction(returnType, Body));
            }
        }

        private void EnsureReturnStatement(StatementSyntax last, Func<ExpressionSyntax, StatementSyntax> resultGen)
        {
            if(this.returnType == ScriptDataType.Void)
            {
                return;
            }

            if (last.TryGetContainingSimpleExpression(out var lastExp))
            {
                Body.Remove(last);
                Body.Add(resultGen(lastExp));
            }
            else if (last.TryGetLeftHandExpression(out var lhsExp))
            {
                Body.Add(resultGen(lhsExp));
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
            Body.Add(SyntaxFactory.ExpressionStatement(exp));

            return this;
        }

        IGenerationContext IGenerationContext.AddExpression(ExpressionSyntax expression) => AddExpression(expression);

        public IStatementContext AddStatement(StatementSyntax statement)
        {
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
