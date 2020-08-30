using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenH2.Core.Scripting.Generation;
using OpenH2.Core.Tags.Scenario;
using System.Collections.Generic;

namespace OpenH2.Core.Scripting.GenerationState
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
            var hasReturn = SyntaxUtil.HasReturnStatement(Body);

            if (scope.IsInStatementContext)
            {
                if(hasReturn == false)
                {
                    SyntaxUtil.CreateReturnStatement(this.returnType, Body, scope.StatementContext.TryCreateResultStatement);
                }

                foreach (var b in Body)
                {
                    scope.StatementContext.AddStatement(b);
                }
            }
            else
            {
                if (hasReturn == false)
                {
                    SyntaxUtil.CreateReturnStatement(this.returnType, Body, (ExpressionSyntax e, out StatementSyntax s) => {
                        s = SyntaxFactory.ReturnStatement(e)
                            .WithAdditionalAnnotations(ScriptGenAnnotations.ResultStatement);
                        return true;
                    });
                }

                scope.Context.AddExpression(
                    SyntaxUtil.CreateImmediatelyInvokedFunction(returnType, Body));
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
