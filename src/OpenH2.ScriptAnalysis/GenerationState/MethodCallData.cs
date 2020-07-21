using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

namespace OpenH2.ScriptAnalysis.GenerationState
{
    public class MethodCallData : IScriptGenerationState
    {
        private readonly string methodName;
        private List<ArgumentSyntax> arguments = new List<ArgumentSyntax>();

        public MethodCallData(string methodName)
        {
            this.methodName = methodName;
        }

        public MethodCallData AddArgument(ExpressionSyntax argument)
        {
            arguments.Add(SyntaxFactory.Argument(argument));
            return this;
        }

        public IScriptGenerationState AddExpression(ExpressionSyntax expression) => AddArgument(expression);

        public InvocationExpressionSyntax GenerateInvocationExpression()
        {
            return SyntaxFactory.InvocationExpression(
                SyntaxFactory.IdentifierName(this.methodName))
                    .WithArgumentList(SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(this.arguments)));
        }
    }
}
