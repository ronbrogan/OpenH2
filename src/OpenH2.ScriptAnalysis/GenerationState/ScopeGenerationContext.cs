using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenH2.Core.Scripting;
using OpenH2.Core.Tags.Scenario;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace OpenH2.ScriptAnalysis.GenerationState
{
    public class ScopeGenerationContext : BaseGenerationContext, IGenerationContext, IStatementContext
    {
        private List<StatementSyntax> Body = new List<StatementSyntax>();

        public override bool CreatesScope => true;
        public override ScriptDataType? OwnDataType { get; }

        public ScopeGenerationContext(ScenarioTag.ScriptSyntaxNode node, ScriptDataType scopeType) : base(node)
        {
            this.OwnDataType = scopeType;
        }

        public IGenerationContext AddExpression(ExpressionSyntax expression)
        {
            this.Body.Add(SyntaxFactory.ExpressionStatement(expression));
            return this;
        }

        public void GenerateInto(Scope scope)
        {
            if (scope.IsInStatementContext)
            {
                foreach (var b in Body)
                {
                    scope.StatementContext.AddStatement(b);
                }
            }
            else if(Body.Count == 1)
            {
                var statement = Body[0];

                if(statement is ExpressionStatementSyntax expStatement)
                {
                    scope.Context.AddExpression(expStatement.Expression);
                }
                else
                {
                    throw new Exception("Generation failed because a singular expression could not be extracted from the body statement");
                }
            }
            else
            {
                scope.Context.AddExpression(
                    SyntaxUtil.CreateImmediatelyInvokedFunction(OwnDataType.Value, Body));
            }
        }

        public IStatementContext AddStatement(StatementSyntax statement)
        {
            this.Body.Add(statement);
            return this;
        }

        public StatementSyntax CreateResultStatement(ExpressionSyntax resultValue)
        {
            // TODO: cast to type if necessary? Add type annotations to all generated expressions?
            return SyntaxFactory.ExpressionStatement(resultValue);
        }
    }
}
