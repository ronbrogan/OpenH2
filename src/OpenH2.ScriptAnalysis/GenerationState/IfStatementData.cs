using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace OpenH2.ScriptAnalysis.GenerationState
{
    internal class IfStatementData : BaseGenerationState, IScriptGenerationState, IHoistableGenerationState, IScopedScriptGenerationState
    {
        private const string AnnotationKind = "IfStatement";
        private const string Annotation_HoistedResultVar = "HoistedResultVar";

        private ExpressionSyntax condition = null;

        private bool addingWhenTrueStatements = true;
        private List<StatementSyntax> whenTrueStatements = new List<StatementSyntax>();
        private List<StatementSyntax> whenFalseStatements = new List<StatementSyntax>();
        private IdentifierNameSyntax resultVariable;

        public IfStatementData()
        {
            var resultVarName = "ifResult_" + this.GetHashCode();

            resultVariable = IdentifierName(resultVarName);
        }

        public IScriptGenerationState AddExpression(ExpressionSyntax expression)
        {
            if(condition == null)
            {
                condition = expression;
            }
            else if(addingWhenTrueStatements)
            {
                whenTrueStatements.Add(SyntaxFactory.ExpressionStatement(expression));
                addingWhenTrueStatements = false;
            }
            else
            {
                whenFalseStatements.Add(SyntaxFactory.ExpressionStatement(expression));
            }

            return this;
        }

        public StatementSyntax CreateResultStatement(ExpressionSyntax resultValue)
        {
            return ExpressionStatement(AssignmentExpression(
                        SyntaxKind.SimpleAssignmentExpression,
                        resultVariable,
                        resultValue))
                .WithAdditionalAnnotations(ScriptGenAnnotations.ResultStatement);
        }

        public IScopedScriptGenerationState AddStatement(StatementSyntax statement)
        {
            if(this.condition == null)
            {
                var gotExp = statement.TryGetContainingSimpleExpression(out var exp);
                Debug.Assert(gotExp, "Unable to get condition expression from statement");

                this.condition = exp;
            }
            else if(addingWhenTrueStatements)
            {
                whenTrueStatements.Add(statement);

                if(statement.IsKind(SyntaxKind.ReturnStatement))
                {
                    addingWhenTrueStatements = false;
                }
            }
            else
            {
                whenFalseStatements.Add(statement);
            }

            return this;
        }

        public StatementSyntax[] GenerateHoistedStatements(out ExpressionSyntax resultVar)
        {
            resultVar = this.resultVariable;
            return GenerateIfStatement(true);
        }

        public StatementSyntax[] GenerateNonHoistedStatements()
        {
            return GenerateIfStatement(false);
        }

        internal StatementSyntax[] GenerateIfStatement(bool isHoisting)
        {
            Debug.Assert(this.condition != null, "Condition expression was not provided");
            Debug.Assert(this.whenTrueStatements.Any(), "WhenTrue was not provided");

            var generatedStatements = new List<StatementSyntax>();

            if (isHoisting)
            {
                generatedStatements.Add(
                    LocalDeclarationStatement(
                        VariableDeclaration(
                            PredefinedType(
                                Token(SyntaxKind.BoolKeyword)))
                        .WithVariables(
                            SingletonSeparatedList(
                                VariableDeclarator(
                                    resultVariable.Identifier)
                                .WithInitializer(
                                    EqualsValueClause(
                                        LiteralExpression(
                                            SyntaxKind.FalseLiteralExpression))))))
                    .WithAdditionalAnnotations(new SyntaxAnnotation(AnnotationKind, Annotation_HoistedResultVar)));

                if(HasResultVarAssignment(whenTrueStatements) == false)
                {
                    InsertResultVarAssignment(whenTrueStatements);
                }

                if(whenFalseStatements.Any() && HasResultVarAssignment(whenFalseStatements) == false)
                {
                    InsertResultVarAssignment(whenFalseStatements);
                }
            }            

            var trueBlock = Block(whenTrueStatements);

            if (whenFalseStatements.Any())
            {
                StatementSyntax falseBlock = Block(whenFalseStatements);

                generatedStatements.Add(
                    IfStatement(this.condition, trueBlock, ElseClause(falseBlock))
                        .WithAdditionalAnnotations(new SyntaxAnnotation(AnnotationKind)));
            }
            else
            {
                generatedStatements.Add(
                    IfStatement(this.condition, trueBlock)
                        .WithAdditionalAnnotations(new SyntaxAnnotation(AnnotationKind)));
            }

            return generatedStatements.ToArray();
        }

        private bool HasResultVarAssignment(IEnumerable<SyntaxNode> roots)
        {
            foreach(var root in roots)
            {
                var checker = new ResultVarChecker(this.resultVariable);

                checker.Visit(root);

                if (checker.HasResultVarAssignment)
                    return true;
            }

            return false;
        }

        private void InsertResultVarAssignment(List<StatementSyntax> statements)
        {
            var lastStatement = statements.Last();

            if (lastStatement.TryGetContainingSimpleExpression(out var simple))
            {
                var updatedLastTrue = ExpressionStatement(AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    resultVariable,
                    simple));

                statements.Remove(lastStatement);
                statements.Add(updatedLastTrue);
            }
            else if (lastStatement.TryGetRightHandExpression(out var rhs))
            {
                var rhsAssignment = ExpressionStatement(AssignmentExpression(
                    SyntaxKind.SimpleAssignmentExpression,
                    resultVariable,
                    rhs));

                statements.Add(rhsAssignment);
            }
        }

        private class ResultVarChecker : CSharpSyntaxWalker
        {
            private readonly IdentifierNameSyntax resultVar;

            public bool HasResultVarAssignment { get; private set; } = false;

            public ResultVarChecker(IdentifierNameSyntax resultVar)
            {
                this.resultVar = resultVar;
            }

            public override void VisitAssignmentExpression(AssignmentExpressionSyntax node)
            {
                if (node.Left == resultVar)
                    HasResultVarAssignment = true;

                base.VisitAssignmentExpression(node);
            }
        }
    }
}