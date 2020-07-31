using OpenH2.Core.Scripting;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace OpenH2.ScriptAnalysis.GenerationState
{
    public class Scope
    {
        public ScriptDataType Type { get; }
        public IGenerationContext Context { get; private set; }

        //private Stack<Scope> childContexts = new Stack<Scope>();
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

        //public void SetContext(IGenerationContext context)
        //{
        //    if(this.Context == null)
        //    {
        //        this.Context = context;

        //        if (context is IStatementContext statementContext)
        //        {
        //            this.StatementContext = statementContext;
        //        }
        //    }
            
        //}

        public void GenerateInto(Scope destination)
        {
            this.Context.GenerateInto(destination);
        }

        public bool IsInStatementContext => this.StatementContext == this.Context;
    }
}
