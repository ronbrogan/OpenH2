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
        private delegate bool ResultVarGenerator(ExpressionSyntax exp, out StatementSyntax statement);

        private readonly ScriptDataType returnType;
        private readonly bool randomizeExecution;

        public override bool CreatesScope => true;

        public List<StatementSyntax> Body { get; } = new List<StatementSyntax>();

        public BeginCallContext(ScenarioTag.ScriptSyntaxNode node, Scope context, ScriptDataType returnType, bool randomizeExecution = false) : base(node)
        {
            this.returnType = returnType;
            this.randomizeExecution = randomizeExecution;
        }

        public void GenerateInto(Scope scope)
        {
            if(scope.IsInStatementContext)
            {
                var lastExp = Body.Last();
                EnsureReturnStatement(lastExp, scope.StatementContext.TryCreateResultStatement);

                foreach (var b in Body)
                {
                    scope.StatementContext.AddStatement(b);
                }
            }
            else
            {
                var lastExp = Body.Last();

                EnsureReturnStatement(lastExp, (ExpressionSyntax e, out StatementSyntax s) => { 
                    s = SyntaxFactory.ReturnStatement(e)
                        .WithAdditionalAnnotations(ScriptGenAnnotations.ResultStatement); 
                    return true;
                });

                scope.Context.AddExpression(
                    SyntaxUtil.CreateImmediatelyInvokedFunction(returnType, Body));
            }
        }

        private void EnsureReturnStatement(StatementSyntax last, ResultVarGenerator resultGen)
        {
            if(this.returnType == ScriptDataType.Void)
            {
                Body.Remove(last);
                Body.Add(last.WithAdditionalAnnotations(ScriptGenAnnotations.FinalScopeStatement));
                return;
            }

            if (last.TryGetContainingSimpleExpression(out var lastExp) && resultGen(lastExp, out var lastStatement))
            {
                Body.Remove(last);
                Body.Add(lastStatement.WithAdditionalAnnotations(ScriptGenAnnotations.FinalScopeStatement));
            }
            else if (last.TryGetLeftHandExpression(out var lhsExp) && resultGen(lhsExp, out var lhsStatement))
            {
                Body.Add(lhsStatement.WithAdditionalAnnotations(ScriptGenAnnotations.FinalScopeStatement));
            }
            else
            {
                var defaultExp = SyntaxFactory.LiteralExpression(
                        SyntaxKind.DefaultLiteralExpression,
                        SyntaxFactory.Token(SyntaxKind.DefaultKeyword));

                if(resultGen(defaultExp, out var defaultStatement))
                {
                    Body.Add(defaultStatement
                    .WithAdditionalAnnotations(ScriptGenAnnotations.FinalScopeStatement)
                    .WithTrailingTrivia(SyntaxFactory.Comment("// Unhandled 'begin' return")));
                }
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

        public bool TryCreateResultStatement(ExpressionSyntax resultValue, out StatementSyntax statement)
        {
            if(this.OwnDataType == ScriptDataType.Void)
            {
                statement = default;
                return false;
            }

            statement = SyntaxFactory.ReturnStatement(resultValue)
                .WithAdditionalAnnotations(ScriptGenAnnotations.ResultStatement);
            return true;
        }
    }
}
