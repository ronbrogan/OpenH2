namespace OpenH2.Core.Scripting.GenerationState
{
    public class Scope
    {
        public ScriptDataType Type { get; }
        public IGenerationContext Context { get; private set; }
        public IStatementContext StatementContext { get; private set; } = null;

        public Scope(ScriptDataType type, IGenerationContext context, IStatementContext nearestStatements)
        {
            this.Type = type;
            this.StatementContext = nearestStatements;
            this.Context = context;
        }

        public Scope CreateChild(IGenerationContext context)
        {
            var child = new Scope(
                context.OwnDataType ?? this.Type, 
                context, 
                context as IStatementContext ?? this.StatementContext);

            return child;
        }

        public void GenerateInto(Scope destination)
        {
            this.Context.GenerateInto(destination);
        }

        public bool IsInStatementContext => this.StatementContext == this.Context;
        public bool SuppressHoisting => this.Context.SuppressHoisting;
    }
}
