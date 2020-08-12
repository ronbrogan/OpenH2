using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenH2.Core.Scripting;
using OpenH2.Core.Tags.Scenario;
using OpenH2.ScriptAnalysis.GenerationState;
using System;

namespace OpenH2.ScriptAnalysis
{
    internal class NavigationPointContext : BaseGenerationContext, IGenerationContext
    {
        private readonly ExpressionSyntax accessor;

        public override ScriptDataType? OwnDataType { get; }

        public NavigationPointContext(ScenarioTag scenario, ScenarioTag.ScriptSyntaxNode node) : base(node)
        {
            this.OwnDataType = node.DataType;

            if (node.NodeString == 0)
            {
                accessor = SyntaxFactory.DefaultExpression(SyntaxUtil.ScriptTypeSyntax(node.DataType));
            }
            else
            {
                var stringVal = SyntaxUtil.GetScriptString(scenario, node);

                accessor = SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    SyntaxFactory.IdentifierName("ScriptEngine"),
                    SyntaxFactory.IdentifierName("NavigationPoints")),
                    SyntaxFactory.IdentifierName(SyntaxUtil.SanitizeIdentifier(stringVal)));
            }
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