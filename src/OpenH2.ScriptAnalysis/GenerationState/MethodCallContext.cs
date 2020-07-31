using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenH2.Core.Scripting;
using OpenH2.Core.Tags.Scenario;
using System.Collections.Generic;

namespace OpenH2.ScriptAnalysis.GenerationState
{
    public class MethodCallContext : BaseGenerationContext, IGenerationContext
    {
        public string MethodName { get; }
        public ScriptDataType ReturnType { get; }

        private List<ArgumentSyntax> arguments = new List<ArgumentSyntax>();

        public override bool CreatesScope => true;

        public MethodCallContext(ScenarioTag.ScriptSyntaxNode node, string methodName, ScriptDataType returnType) : base(node)
        {
            this.MethodName = methodName;
            this.ReturnType = returnType;
        }

        public MethodCallContext AddArgument(ExpressionSyntax argument)
        {
            arguments.Add(SyntaxFactory.Argument(argument));
            return this;
        }

        public IGenerationContext AddExpression(ExpressionSyntax expression) => AddArgument(expression);

        public void GenerateInto(Scope scope)
        {
            scope.Context.AddExpression(SyntaxFactory.InvocationExpression(
                SyntaxFactory.IdentifierName(this.MethodName))
                    .WithArgumentList(SyntaxFactory.ArgumentList(
                        SyntaxFactory.SeparatedList(this.arguments))));
        }
    }
}
