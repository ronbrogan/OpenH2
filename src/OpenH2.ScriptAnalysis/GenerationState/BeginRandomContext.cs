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
    public class BeginRandomContext : BaseGenerationContext, IGenerationContext, IStatementContext
    {
        public override ScriptDataType? OwnDataType { get; }
        public override bool CreatesScope => true;

        private List<ExpressionSyntax> subexpressions = new List<ExpressionSyntax>();

        private static TypeSyntax[] IifeTypes = new TypeSyntax[]
        {
            SyntaxFactory.IdentifierName("Action")
        };

        private static TypeSyntax BaseFuncType = SyntaxFactory.GenericName("Func");

        public BeginRandomContext(ScenarioTag.ScriptSyntaxNode node, ScriptDataType type) : base(node)
        {
            this.OwnDataType = type;
        }

        public IGenerationContext AddExpression(ExpressionSyntax expression)
        {
            subexpressions.Add(expression);
            return this;
        }

        public void GenerateInto(Scope scope)
        {
            List<ExpressionSyntax> lambdas = new List<ExpressionSyntax>();

            foreach(var exp in subexpressions)
            {
                // Unwrap anything added as new Action(() => {}) or new Func<T>(() => {})
                if (exp is InvocationExpressionSyntax invocation
                    && invocation.Expression is ObjectCreationExpressionSyntax creation)
                {
                    if(IifeTypes.Any(t => creation.Type.IsEquivalentTo(t)))
                    {
                        lambdas.Add(creation.ArgumentList.Arguments[0].Expression);
                        continue;
                    }

                    if(creation.Type is GenericNameSyntax gen 
                        && gen.WithTypeArgumentList(SyntaxFactory.TypeArgumentList()).IsEquivalentTo(BaseFuncType))
                    {
                        lambdas.Add(creation.ArgumentList.Arguments[0].Expression);
                        continue;
                    }
                }
                
                lambdas.Add(SyntaxFactory.ParenthesizedLambdaExpression(exp));
            }

            scope.Context.AddExpression(SyntaxFactory.InvocationExpression(
                SyntaxFactory.IdentifierName("begin_random"))
                    .AddArgumentListArguments(lambdas.Select(SyntaxFactory.Argument).ToArray()));
        }

        public IStatementContext AddStatement(StatementSyntax statement)
        {
            if (statement is ExpressionStatementSyntax exp)
            {
                subexpressions.Add(exp.Expression);
            }
            else
            {
                var returnType = ScriptDataType.Void;

                var annotations = statement.GetAnnotations(ScriptGenAnnotations.TypeAnnotationKind);

                if (annotations.Any())
                {
                    returnType = (ScriptDataType)int.Parse(annotations.First().Data);
                }

                subexpressions.Add(SyntaxUtil.CreateImmediatelyInvokedFunction(returnType, new[] { statement }));
            }

            return this;
        }

        public bool TryCreateResultStatement(ExpressionSyntax resultValue, out StatementSyntax statement)
        {
            if (this.OwnDataType == ScriptDataType.Void)
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
