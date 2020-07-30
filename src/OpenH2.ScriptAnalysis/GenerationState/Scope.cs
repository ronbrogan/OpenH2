using OpenH2.Core.Scripting;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace OpenH2.ScriptAnalysis.GenerationState
{
    public class Scope
    {
        public ScriptDataType Type { get; }
        public IExpressionContext Context { get; private set; }

        private Stack<IExpressionContext> childContexts = new Stack<IExpressionContext>();
        public IStatementContext StatementContext { get; private set; } = null;

        public Scope(ScriptDataType type, IStatementContext nearestStatements)
        {
            this.Type = type;
            this.StatementContext = nearestStatements;
        }

        public Scope(ScriptDataType type, IExpressionContext context, IStatementContext nearestStatements)
        {
            this.Type = type;
            this.StatementContext = nearestStatements;
            this.Context = context;
        }

        public void AddContext(IExpressionContext context)
        {
            if(this.Context == null)
            {
                this.Context = context;

                if (context is IStatementContext statementContext)
                {
                    this.StatementContext = statementContext;
                }
            }
            else
            {
                this.childContexts.Push(context);
            }
        }

        public void GenerateInto(Scope destination)
        {
            while(this.childContexts.TryPop(out var context))
            {
                context.GenerateInto(this);
            }

            this.Context.GenerateInto(destination);
        }

        public bool IsInStatementContext => this.StatementContext == this.Context;
    }
}
