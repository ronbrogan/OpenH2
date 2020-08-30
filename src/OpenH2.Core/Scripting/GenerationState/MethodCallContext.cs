using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenH2.Core.Scripting.Generation;
using OpenH2.Core.Tags.Scenario;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OpenH2.Core.Scripting.GenerationState
{
    public class MethodCallContext : BaseGenerationContext, IGenerationContext
    {
        public string MethodName { get; }
        public ScriptDataType ReturnType { get; }

        private List<ArgumentSyntax> arguments = new List<ArgumentSyntax>();
        private List<ScriptDataType?> argumentTypes = new List<ScriptDataType?>();

        public override bool CreatesScope => true;

        public MethodCallContext(ScenarioTag.ScriptSyntaxNode node, string methodName, ScriptDataType returnType) : base(node)
        {
            this.MethodName = methodName;
            this.ReturnType = returnType;
        }

        public MethodCallContext AddArgument(ExpressionSyntax argument)
        {
            arguments.Add(SyntaxFactory.Argument(argument));
            
            if(SyntaxUtil.TryGetTypeOfExpression(argument, out var t))
            {
                argumentTypes.Add(t);
            }
            else
            {
                throw new System.Exception("Couldn't determine argument type");
            }

            return this;
        }

        public IGenerationContext AddExpression(ExpressionSyntax expression) => AddArgument(expression);

        public void GenerateInto(Scope scope)
        {
            var invocationExpression = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                SyntaxFactory.IdentifierName("Engine"),
                SyntaxFactory.IdentifierName(this.MethodName));

            ExpressionSyntax invocation = SyntaxFactory.InvocationExpression(invocationExpression)
                    .WithArgumentList(SyntaxFactory.ArgumentList(
                        SyntaxFactory.SeparatedList(this.arguments)))
                    .WithAdditionalAnnotations(ScriptGenAnnotations.TypeAnnotation(this.ReturnType));

            var tempArgs = new List<Type>();

            foreach (var t in argumentTypes)
            {
                if(t.HasValue && SyntaxUtil.TryGetTypeFromScriptType(t.Value, out var T))
                {
                    tempArgs.Add(T);
                }
                else
                {
                    break;
                }
            }

            if(tempArgs.Count == argumentTypes.Count)
            {
                // Do full overload match
                var method = typeof(IScriptEngine).GetMethod(this.MethodName, 
                    BindingFlags.Public | BindingFlags.Instance,
                    null,
                    tempArgs.ToArray(), 
                    null);

                if(method != null)
                {
                    SyntaxUtil.AwaitIfNeeded(method, ref invocation, out var materializedReturnType);

                    if (SyntaxUtil.TryGetScriptTypeFromType(materializedReturnType, out var fromType))
                    {
                        // Insert cast to destination
                        invocation = SyntaxUtil.CreateCast(fromType, this.ReturnType, invocation)
                            .WithAdditionalAnnotations(ScriptGenAnnotations.TypeAnnotation(this.ReturnType));
                    }
                }
            }
            else
            {
                // Fallback to name only lookup
                var scriptEngineMethods = typeof(IScriptEngine).GetMethods().Where(m => m.Name == this.MethodName);

                if (scriptEngineMethods.Any())
                {
                    //var hasOverload = scriptEngineMethods.Any(m => m.ReturnType == destinationType);

                    //if (hasOverload == false)
                    {
                        var method = scriptEngineMethods.First();

                        SyntaxUtil.AwaitIfNeeded(method, ref invocation, out var materializedReturnType);

                        if(SyntaxUtil.TryGetScriptTypeFromType(materializedReturnType, out var fromType))
                        {
                            // Insert cast to destination
                            invocation = SyntaxUtil.CreateCast(fromType, this.ReturnType, invocation)
                                .WithAdditionalAnnotations(ScriptGenAnnotations.TypeAnnotation(this.ReturnType));
                        }
                    }
                }
            }            

            scope.Context.AddExpression(invocation);
        }
    }
}
