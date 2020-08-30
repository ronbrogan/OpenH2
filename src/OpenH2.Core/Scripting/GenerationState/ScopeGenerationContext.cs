using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenH2.Core.Tags.Scenario;

namespace OpenH2.Core.Scripting.GenerationState
{
    public class ScopeGenerationContext : BaseGenerationContext, IGenerationContext
    {
        //private List<StatementSyntax> Body = new List<StatementSyntax>();
        private readonly Scope containingScope;

        public override bool CreatesScope => true;
        public override bool SuppressHoisting => containingScope.SuppressHoisting;
        public override ScriptDataType? OwnDataType { get; }

        public ScopeGenerationContext(ScenarioTag.ScriptSyntaxNode node, Scope containingScope) : base(node)
        {
            this.OwnDataType = node.DataType;
            this.containingScope = containingScope;
        }

        public IGenerationContext AddExpression(ExpressionSyntax expression)
        {
            this.containingScope.Context.AddExpression(expression);
            return this;
        }

        public void GenerateInto(Scope scope)
        {
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
