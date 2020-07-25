using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace OpenH2.ScriptAnalysis.GenerationState
{
    internal class IfStatementData : IScriptGenerationState
    {
        private ExpressionSyntax condition = null;
        private ExpressionSyntax whenTrue = null;
        private ExpressionSyntax whenFalse = null;

        public IfStatementData()
        {
        }


        public IScriptGenerationState AddExpression(ExpressionSyntax expression)
        {
            if(condition == null)
            {
                condition = expression;
            }
            else if(whenTrue == null)
            {
                whenTrue = expression;
            }
            else if(whenFalse == null)
            {
                whenFalse = expression;
            }
            else
            {
                throw new Exception("Too many expression provided to IfStatementData");
            }

            return this;
        }

        internal StatementSyntax[] GenerateIfStatement(bool isInStatementScope, out ExpressionSyntax resultVariable)
        {
            Debug.Assert(this.condition != null, "Condition expression was not provided");
            Debug.Assert(this.whenTrue != null, "WhenTrue expression was not provided");

            var resultVarName = "ifResult_" + this.GetHashCode();

            resultVariable = IdentifierName(resultVarName);

            var resultStatements = new List<StatementSyntax>();

            ExpressionSyntax whenTrueExpression;

            if (isInStatementScope)
            {
                whenTrueExpression = this.whenTrue;
            }
            else
            {
                resultStatements.Add(
                    LocalDeclarationStatement(
                        VariableDeclaration(
                            PredefinedType(
                                Token(SyntaxKind.BoolKeyword)))
                        .WithVariables(
                            SingletonSeparatedList(
                                VariableDeclarator(
                                    Identifier(resultVarName))
                                .WithInitializer(
                                    EqualsValueClause(
                                        LiteralExpression(
                                            SyntaxKind.FalseLiteralExpression)))))));

                whenTrueExpression = AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    resultVariable,
                    this.whenTrue);
            }            

            var trueBlock = Block(ExpressionStatement(whenTrueExpression));

            resultStatements.Add(IfStatement(this.condition, trueBlock));

            return resultStatements.ToArray();
        }
    }
}