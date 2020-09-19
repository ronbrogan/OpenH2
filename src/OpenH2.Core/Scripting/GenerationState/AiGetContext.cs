using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenH2.Core.Scripting.Generation;
using OpenH2.Core.Tags.Scenario;
using System;

namespace OpenH2.Core.Scripting.GenerationState
{
    public class AiGetContext : BaseGenerationContext, IGenerationContext
    {
        private readonly ExpressionSyntax accessor;

        public override ScriptDataType? OwnDataType { get; }

        public AiGetContext(ScenarioTag scenario, ScenarioTag.ScriptSyntaxNode node, MemberNameRepository nameRepo) : base(node)
        {
            this.OwnDataType = node.DataType;

            if (node.NodeString == 0)
            {
                accessor = SyntaxFactory.DefaultExpression(SyntaxUtil.ScriptTypeSyntax(node.DataType));
            }
            else
            {
                var stringVal = SyntaxUtil.GetScriptString(scenario, node);

                var slashIndex = stringVal.IndexOf('/');

                if (slashIndex > 0)
                {
                    // It's a squad member accessor
                    var squadName = stringVal.Substring(0, slashIndex);
                    var memberName = stringVal.Substring(slashIndex + 1);

                    if (nameRepo.TryGetName(squadName, node.DataType, node.NodeData_H16, out var finalSquad)
                        && nameRepo.NestedRepos.TryGetValue(finalSquad, out var nestedRepo)
                        && nestedRepo.TryGetName(memberName, node.DataType, node.NodeData_H16, out var finalProp))
                    {
                        accessor = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.IdentifierName(SyntaxUtil.SanitizeIdentifier(finalSquad)),
                            SyntaxFactory.IdentifierName(SyntaxUtil.SanitizeIdentifier(finalProp)));
                    }
                    else
                    {
                        accessor = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                            SyntaxFactory.IdentifierName(SyntaxUtil.SanitizeIdentifier(squadName)),
                            SyntaxFactory.IdentifierName(SyntaxUtil.SanitizeIdentifier(memberName)));
                    }                    
                }
                else
                {
                    // Ambiguous reference to either a squad or squad group...?
                    if (nameRepo.TryGetName(stringVal, node.DataType, node.NodeData_H16, out var finalSquad))
                    {
                        if(nameRepo.NestedRepos.TryGetValue(finalSquad, out var nestedRepo))
                        {
                            accessor = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                SyntaxFactory.IdentifierName(SyntaxUtil.SanitizeIdentifier(finalSquad)),
                                SyntaxFactory.IdentifierName(SyntaxUtil.SanitizeIdentifier("Squad")));
                        }
                        else
                        {
                            accessor = SyntaxFactory.IdentifierName(finalSquad);
                        }
                    }
                    else
                    {
                        accessor = SyntaxFactory.IdentifierName(SyntaxUtil.SanitizeIdentifier(stringVal));
                    }
                }
            }

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