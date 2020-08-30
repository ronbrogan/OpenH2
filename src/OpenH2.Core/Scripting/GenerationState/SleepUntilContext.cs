using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenH2.Core.Scripting.Generation;
using OpenH2.Core.Tags.Scenario;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace OpenH2.Core.Scripting.GenerationState
{
    public class SleepUntilContext : BaseGenerationContext, IGenerationContext
    {
        private static TypeSyntax FuncBoolType = SyntaxFactory.GenericName("Func")
            .AddTypeArgumentListArguments(SyntaxFactory.PredefinedType(SyntaxFactory.Token(SyntaxKind.BoolKeyword)));

        public ScriptDataType ReturnType { get; }

        private List<ArgumentSyntax> arguments = new List<ArgumentSyntax>();

        public override bool CreatesScope => true;
        public override bool SuppressHoisting => true;

        public SleepUntilContext(ScenarioTag.ScriptSyntaxNode node, ScriptDataType returnType) : base(node)
        {
            this.ReturnType = returnType;
        }

        public SleepUntilContext AddArgument(ExpressionSyntax argument)
        {
            arguments.Add(SyntaxFactory.Argument(argument));
            return this;
        }

        public IGenerationContext AddExpression(ExpressionSyntax expression) => AddArgument(expression);

        public void GenerateInto(Scope scope)
        {
            var checkArg = this.arguments.First();

            ExpressionSyntax checkExpression;

            if(checkArg.Expression is InvocationExpressionSyntax invocation
                && invocation.Expression is ObjectCreationExpressionSyntax creation
                && creation.Type.IsEquivalentTo(FuncBoolType))
            {
                Debug.Assert(invocation.ArgumentList.Arguments.Count == 0, "Invocation with arguments isn't allowed");
                checkExpression = creation.ArgumentList.Arguments[0].Expression;
            }
            else
            {
                checkExpression = SyntaxFactory.ParenthesizedLambdaExpression(checkArg.Expression)
                    .WithAsyncKeyword(SyntaxFactory.Token(SyntaxKind.AsyncKeyword));
            }

            var finalArgs = this.arguments.ToArray();
            finalArgs[0] = SyntaxFactory.Argument(checkExpression);

            scope.Context.AddExpression(SyntaxFactory.AwaitExpression(SyntaxFactory.InvocationExpression(
                SyntaxFactory.IdentifierName("sleep_until"))
                    .AddArgumentListArguments(finalArgs)
                ).WithAdditionalAnnotations(ScriptGenAnnotations.TypeAnnotation(this.ReturnType)));
        }
    }
}
