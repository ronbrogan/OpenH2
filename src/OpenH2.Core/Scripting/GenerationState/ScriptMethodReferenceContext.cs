using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenH2.Core.Scripting.Generation;
using OpenH2.Core.Tags.Scenario;
using System;
using System.Diagnostics;

namespace OpenH2.Core.Scripting.GenerationState
{
    public class ScriptMethodReferenceContext : BaseGenerationContext, IGenerationContext
    {
        private readonly ExpressionSyntax accessor;

        public override ScriptDataType? OwnDataType { get; }

        public ScriptMethodReferenceContext(ScenarioTag scenario, ScenarioTag.ScriptSyntaxNode node, MemberNameRepository nameRepo) : base(node)
        {
            this.OwnDataType = node.DataType;

            if (node.NodeString == 0)
            {
                accessor = SyntaxFactory.DefaultExpression(SyntaxUtil.ScriptTypeSyntax(node.DataType));
            }
            else
            {
                var stringVal = SyntaxUtil.GetScriptString(scenario, node);

                if(stringVal == "none")
                {
                    accessor = SyntaxFactory.DefaultExpression(SyntaxUtil.ScriptTypeSyntax(node.DataType));
                }
                else
                {
                    if(nameRepo.TryGetName(stringVal, node.DataType.ToString(), node.NodeData_H16, out var finalName))
                    {
                        accessor = SyntaxFactory.IdentifierName(finalName);
                    }
                    else
                    {
                        accessor = SyntaxFactory.IdentifierName(SyntaxUtil.SanitizeIdentifier(stringVal));
                    }
                }
            }

            accessor = SyntaxFactory.ObjectCreationExpression(SyntaxFactory.ParseTypeName("ScriptMethodReference"))
                .AddArgumentListArguments(SyntaxFactory.Argument(accessor));

            accessor = accessor.WithAdditionalAnnotations(ScriptGenAnnotations.TypeAnnotation(node.DataType));
        }

        public IGenerationContext AddExpression(ExpressionSyntax expression)
        {
            throw new NotImplementedException();
        }

        public void GenerateInto(Scope scope)
        {
            scope.Context.AddExpression(accessor);
        }
    }
}
