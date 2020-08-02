using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenH2.Core.Scripting;
using OpenH2.Core.Tags.Scenario;

namespace OpenH2.ScriptAnalysis.GenerationState
{
    public class ScopeGenerationContext : BaseGenerationContext, IGenerationContext
    {
        //private List<StatementSyntax> Body = new List<StatementSyntax>();
        private readonly Scope containingScope;

        public override bool CreatesScope => true;
        public override ScriptDataType? OwnDataType { get; }

        public ScopeGenerationContext(ScenarioTag.ScriptSyntaxNode node, Scope containingScope) : base(node)
        {
            this.OwnDataType = node.DataType;
            this.containingScope = containingScope;
        }

        public IGenerationContext AddExpression(ExpressionSyntax expression)
        {
            //this.Body.Add(SyntaxFactory.ExpressionStatement(expression));
            this.containingScope.Context.AddExpression(expression);
            return this;
        }

        public void GenerateInto(Scope scope)
        {
            //scope.Context.AddExpression(SyntaxFactory.BaseExpression().WithLeadingTrivia(SyntaxFactory.Comment($"// Scope<{this.OwnDataType}>")));
            return;

            //if (scope.IsInStatementContext)
            //{
            //    foreach (var b in Body)
            //    {
            //        scope.StatementContext.AddStatement(b);
            //    }
            //}
            //else if(Body.Count == 1)
            //{
            //    var statement = Body[0];
            //
            //    if(statement is ExpressionStatementSyntax expStatement)
            //    {
            //        scope.Context.AddExpression(expStatement.Expression);
            //    }
            //    else
            //    {
            //        throw new Exception("Generation failed because a singular expression could not be extracted from the body statement");
            //    }
            //}
            //else
            //{
            //    scope.Context.AddExpression(
            //        SyntaxUtil.CreateImmediatelyInvokedFunction(OwnDataType.Value, Body));
            //}
        }

        public class StatementContext : ScopeGenerationContext, IStatementContext
        {
            public StatementContext(ScenarioTag.ScriptSyntaxNode node, Scope containingScope) : base (node, containingScope)
            {
            }

            public IStatementContext AddStatement(StatementSyntax statement)
            {
                this.containingScope.StatementContext.AddStatement(statement);
                return this;
            }

            public bool TryCreateResultStatement(ExpressionSyntax resultValue, out StatementSyntax statement)
            {
                // TODO: cast to type if necessary? Add type annotations to all generated expressions?
                return this.containingScope.StatementContext.TryCreateResultStatement(resultValue, out statement);
            }
        }
    }
}
