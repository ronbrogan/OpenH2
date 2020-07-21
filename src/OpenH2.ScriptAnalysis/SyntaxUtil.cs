using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenH2.Core.Extensions;
using OpenH2.Core.Scripting;
using OpenH2.Core.Tags.Scenario;
using System;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace OpenH2.ScriptAnalysis
{
    public static class SyntaxUtil
    {
        public static FieldDeclarationSyntax CreateFieldDeclaration(ScenarioTag tag, ScenarioTag.ScriptVariableDefinition variable)
        {
            return variable.DataType switch
            {
                ScriptDataType.Float => CreateDeclaration(SyntaxKind.FloatKeyword, LiteralExpression(BitConverter.Int32BitsToSingle((int)variable.Value_32))),
                ScriptDataType.Int => CreateDeclaration(SyntaxKind.IntKeyword, LiteralExpression((int)variable.Value_32)),
                ScriptDataType.Boolean => CreateDeclaration(SyntaxKind.BoolKeyword, LiteralExpression(variable.Value_B3 == 1)),
                ScriptDataType.Short => CreateDeclaration(SyntaxKind.ShortKeyword, LiteralExpression(variable.Value_H16)),
                ScriptDataType.String => CreateDeclaration(SyntaxKind.StringKeyword, LiteralExpression(((Span<byte>)tag.ScriptStrings).ReadStringStarting((int)variable.Value_H16))),

                _ => throw new NotImplementedException(),
            };

            FieldDeclarationSyntax CreateDeclaration(SyntaxKind keyword, LiteralExpressionSyntax value)
            {
                return FieldDeclaration(VariableDeclaration(PredefinedType(Token(keyword)))
                    .WithVariables(SingletonSeparatedList<VariableDeclaratorSyntax>(
                        VariableDeclarator(variable.Description)
                        .WithInitializer(EqualsValueClause(value)))));
            }
        }

        public static LiteralExpressionSyntax LiteralExpression(ScenarioTag tag, ScenarioTag.ScriptSyntaxNode node)
        {
            return node.DataType switch
            {
                ScriptDataType.Float => LiteralExpression(BitConverter.Int32BitsToSingle((int)node.NodeData_32)),
                ScriptDataType.Int => LiteralExpression((int)node.NodeData_32),
                ScriptDataType.Boolean=> LiteralExpression(node.NodeData_B0 == 1),
                ScriptDataType.Short => LiteralExpression(node.NodeData_H16),
                ScriptDataType.String => LiteralExpression(((Span<byte>)tag.ScriptStrings).ReadStringStarting(node.NodeString)),

                _ => throw new NotImplementedException(),
            };
        }

        public static LiteralExpressionSyntax LiteralExpression<T>(T value)
        {
            return value switch
            {
                int i => SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(i)),
                short s => SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(s)),
                ushort s => SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(s)),
                float f => SyntaxFactory.LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(f)),
                string s => SyntaxFactory.LiteralExpression(SyntaxKind.StringLiteralExpression, Literal(s)),
                true => SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression),
                false => SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression),

                _ => throw new NotImplementedException(),
            };
        }
    }
}
