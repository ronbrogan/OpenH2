using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenH2.Core.Scripting;
using System.Collections.Generic;

namespace OpenH2.ScriptAnalysis.GenerationState
{
    public class MethodCallData : BaseGenerationState, IScriptGenerationState
    {
        public string MethodName { get; }
        public ScriptDataType ReturnType { get; }

        private List<ArgumentSyntax> arguments = new List<ArgumentSyntax>();

        public MethodCallData(string methodName, ScriptDataType returnType)
        {
            this.MethodName = methodName;
            this.ReturnType = returnType;
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
                SyntaxFactory.IdentifierName(this.MethodName))
                    .WithArgumentList(SyntaxFactory.ArgumentList(SyntaxFactory.SeparatedList(this.arguments)));
        }
    }
}
