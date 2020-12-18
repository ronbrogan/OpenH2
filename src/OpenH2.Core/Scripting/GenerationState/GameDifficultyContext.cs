using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenH2.Core.Tags.Scenario;
using System;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace OpenH2.Core.Scripting.GenerationState
{
    public class GameDifficultyContext : BaseGenerationContext, IGenerationContext
    {
        private ExpressionSyntax literal;

        public override ScriptDataType? OwnDataType { get; }

        public GameDifficultyContext(ScenarioTag scenario, ScenarioTag.ScriptSyntaxNode node, Scope containing) : base(node)
        {
            this.OwnDataType = node.DataType;

            var difficulty = node.NodeData_H16 switch
            {
                0 => nameof(GameDifficulty.Easy),
                1 => nameof(GameDifficulty.Normal),
                2 => nameof(GameDifficulty.Heroic),
                3 => nameof(GameDifficulty.Legendary),
                _ => throw new NotSupportedException()
            };

            this.literal = InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                IdentifierName(nameof(GameDifficulty)),
                IdentifierName(difficulty)));
        }

        public IGenerationContext AddExpression(ExpressionSyntax expression)
        {
            throw new NotImplementedException();
        }

        public void GenerateInto(Scope scope)
        {
            scope.Context.AddExpression(literal);
        }
    }
}
