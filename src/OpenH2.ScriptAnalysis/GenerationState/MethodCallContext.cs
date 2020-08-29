using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenH2.Core.Scripting;
using OpenH2.Core.Tags.Scenario;
using OpenH2.Engine.Scripting;
using OpenH2.Foundation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace OpenH2.ScriptAnalysis.GenerationState
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
            ExpressionSyntax invocation = SyntaxFactory.InvocationExpression(
                SyntaxFactory.IdentifierName(this.MethodName))
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
                var method = typeof(ScriptEngine).GetMethod(this.MethodName, 
                    BindingFlags.Public | BindingFlags.Static,
                    null,
                    tempArgs.ToArray(), 
                    null);

                if(method != null)
                {
                    SyntaxUtil.AwaitIfNeeded(method, ref invocation, out var materializedReturnType);

                    if(SyntaxUtil.TryGetTypeFromScriptType(this.ReturnType, out var destinationType))
                    {
                        // Insert cast to destination
                        invocation = SyntaxUtil.CreateCast(materializedReturnType, destinationType, invocation)
                            .WithAdditionalAnnotations(ScriptGenAnnotations.TypeAnnotation(this.ReturnType));
                    }
                }
            }
            else
            {
                // Fallback to name only lookup
                var scriptEngineMethods = typeof(ScriptEngine).GetMethods().Where(m => m.Name == this.MethodName);

                if (scriptEngineMethods.Any())
                {
                    //var hasOverload = scriptEngineMethods.Any(m => m.ReturnType == destinationType);

                    //if (hasOverload == false)
                    {
                        var method = scriptEngineMethods.First();

                        SyntaxUtil.AwaitIfNeeded(method, ref invocation, out var materializedReturnType);

                        if (SyntaxUtil.TryGetTypeFromScriptType(this.ReturnType, out var destinationType))
                        {
                            // Insert cast to destination
                            invocation = SyntaxUtil.CreateCast(materializedReturnType, destinationType, invocation)
                                .WithAdditionalAnnotations(ScriptGenAnnotations.TypeAnnotation(this.ReturnType));
                        }
                    }
                }
            }            

            scope.Context.AddExpression(invocation);
        }
    }
}
