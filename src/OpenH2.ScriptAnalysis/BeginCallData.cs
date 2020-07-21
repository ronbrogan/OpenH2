using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenH2.ScriptAnalysis
{
    public class BeginCallData
    {
        public List<StatementSyntax> Body { get; }

        public BeginCallData(List<StatementSyntax> body)
        {
            this.Body = body;
        }

        internal StatementSyntax GenerateInvocationStatement()
        {
            var lastStatement = Body.Last();

            if (lastStatement.ChildNodes().First() is ExpressionSyntax lastExpression)
            {
                var returnStatement = SyntaxFactory.ReturnStatement(lastExpression);
                Body.Remove(lastStatement);
                Body.Add(returnStatement);
            }

            return SyntaxFactory.ExpressionStatement(
                SyntaxFactory.InvocationExpression(
                    SyntaxFactory.ParenthesizedLambdaExpression(
                        SyntaxFactory.Block(
                            SyntaxFactory.List(Body)))));
        }

        internal object AddExpression(ExpressionSyntax literal)
        {
            return this;
        }
    }
}
