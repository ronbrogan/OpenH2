using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenH2.ScriptAnalysis.GenerationState
{
    public class BeginCallData : IScriptGenerationState
    {
        public List<StatementSyntax> Body = new List<StatementSyntax>();

        public BeginCallData()
        {
        }

        internal ExpressionSyntax GenerateInvocationStatement()
        {
            var lastStatement = Body.Last();

            if (lastStatement.ChildNodes().First() is ExpressionSyntax lastExpression)
            {
                var returnStatement = SyntaxFactory.ReturnStatement(lastExpression);
                Body.Remove(lastStatement);
                Body.Add(returnStatement);
            }

            return SyntaxFactory.InvocationExpression(
                    SyntaxFactory.ParenthesizedLambdaExpression(
                        SyntaxFactory.Block(
                            SyntaxFactory.List(Body))));
        }

        internal BeginCallData AddExpression(ExpressionSyntax exp)
        {
            Body.Add(SyntaxFactory.ExpressionStatement(exp));

            return this;
        }

        IScriptGenerationState IScriptGenerationState.AddExpression(ExpressionSyntax expression) => AddExpression(expression);
    }
}
