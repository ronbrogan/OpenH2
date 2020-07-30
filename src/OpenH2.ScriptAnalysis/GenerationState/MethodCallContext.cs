using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenH2.Core.Scripting;
using System.Collections.Generic;

namespace OpenH2.ScriptAnalysis.GenerationState
{
    public class MethodCallContext : BaseGenerationContext, IExpressionContext
    {
        public string MethodName { get; }
        public ScriptDataType ReturnType { get; }

        private Stack<ArgumentSyntax> arguments = new Stack<ArgumentSyntax>();

        public MethodCallContext(string methodName, ScriptDataType returnType)
        {
            this.MethodName = methodName;
            this.ReturnType = returnType;
        }

        public MethodCallContext AddArgument(ExpressionSyntax argument)
        {
            arguments.Push(SyntaxFactory.Argument(argument));
            return this;
        }

        public IExpressionContext AddExpression(ExpressionSyntax expression) => AddArgument(expression);

        public void GenerateInto(Scope scope)
        {
            scope.Context.AddExpression(SyntaxFactory.InvocationExpression(
                SyntaxFactory.IdentifierName(this.MethodName))
                    .WithArgumentList(SyntaxFactory.ArgumentList(
                        SyntaxFactory.SeparatedList(this.arguments))));
        }
    }
}
