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
    internal class IfStatementContext : BaseGenerationContext, IExpressionContext, IStatementContext
    {
        private const string AnnotationKind = "IfStatement";
        private const string Annotation_HoistedResultVar = "HoistedResultVar";
        private readonly Scope containingState;

        //private ExpressionSyntax condition = null;
        //private bool addingWhenTrueStatements = true;
        //private List<StatementSyntax> whenTrueStatements = new List<StatementSyntax>();
        //private List<StatementSyntax> whenFalseStatements = new List<StatementSyntax>();
        private IdentifierNameSyntax resultVariable;

        private Stack<StatementSyntax> statements = new Stack<StatementSyntax>();

        public IfStatementContext(Scope containingState)
        {
            var resultVarName = "ifResult_" + this.GetHashCode();

            resultVariable = IdentifierName(resultVarName);
            this.containingState = containingState;
        }

        public IExpressionContext AddExpression(ExpressionSyntax expression)
        {
            statements.Push(ExpressionStatement(expression));

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

        public IStatementContext AddStatement(StatementSyntax statement)
        {
            statements.Push(statement);

            return this;
        }

        public StatementSyntax[] GetInnerStatements()
        {
            throw new NotImplementedException();
        }

        public void GenerateInto(Scope scope)
        {
            var isHoisting = !scope.IsInStatementContext;

            var conditionStatement = statements.Pop();

            Debug.Assert(conditionStatement is ExpressionStatementSyntax, "Condition expression was not provided");

            var condition = ((ExpressionStatementSyntax)conditionStatement).Expression;

            var whenTrueStatements = new List<StatementSyntax>();
            var whenFalseStatements = new List<StatementSyntax>();

            while (statements.TryPop(out var statement))
            {
                whenTrueStatements.Add(statement);

                if(statement is ReturnStatementSyntax)
                {
                    break;
                }
            }

            while(statements.TryPop(out var statement))
            {
                whenFalseStatements.Add(statement);
            }
            

            Debug.Assert(whenTrueStatements.Any(), "WhenTrue was not provided");

            var generatedStatements = new List<StatementSyntax>();

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

            if (isHoisting)
            {
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
                    IfStatement(condition, trueBlock, ElseClause(falseBlock))
                        .WithAdditionalAnnotations(new SyntaxAnnotation(AnnotationKind)));
            }
            else
            {
                generatedStatements.Add(
                    IfStatement(condition, trueBlock)
                        .WithAdditionalAnnotations(new SyntaxAnnotation(AnnotationKind)));
            }

            foreach(var statement in generatedStatements)
            {
                scope.StatementContext.AddStatement(statement);
            }

            scope.Context.AddExpression(this.resultVariable);
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