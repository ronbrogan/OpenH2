using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenH2.Core.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenH2.ScriptAnalysis.GenerationState
{
    public class BeginCallData : BaseGenerationState, IScriptGenerationState, IScopedScriptGenerationState
    {
        private readonly ScriptDataType returnType;

        public List<StatementSyntax> Body { get; set; } = new List<StatementSyntax>();

        public BeginCallData(ScriptDataType returnType)
        {
            this.returnType = returnType;
        }

        internal IScriptGenerationState Generate(IScriptGenerationState state)
        {
            if(state is IScopedScriptGenerationState scoped)
            {
                EnsureReturnStatement(scoped.CreateResultStatement);

                foreach (var b in Body)
                {
                    scoped.AddStatement(b);
                }

                return state;
            }

            //if(Body.Count == 1 && Body[0].ChildNodes().First() is ExpressionSyntax onlyExpression)
            //{
            //    return onlyExpression;
            //}

            EnsureReturnStatement(s => SyntaxFactory.ReturnStatement(s));
            state.AddExpression(
                SyntaxUtil.CreateImmediatelyInvokedFunction(returnType, Body));

            return state;
        }

        private void EnsureReturnStatement(Func<ExpressionSyntax, StatementSyntax> resultGen)
        {
            var last = Body.Last();
            if (last.TryGetContainingSimpleExpression(out var lastExp))
            {
                Body.Remove(last);
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

        public BeginCallData AddExpression(ExpressionSyntax exp)
        {
            Body.Add(SyntaxFactory.ExpressionStatement(exp));

            return this;
        }

        IScriptGenerationState IScriptGenerationState.AddExpression(ExpressionSyntax expression) => AddExpression(expression);

        public IScopedScriptGenerationState AddStatement(StatementSyntax statement)
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
