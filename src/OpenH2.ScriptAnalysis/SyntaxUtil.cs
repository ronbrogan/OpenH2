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
        public static TypeSyntax ScriptTypeSyntax(ScriptDataType dataType)
        {
            return dataType switch
            {
                ScriptDataType.Float => PredefinedType(Token(SyntaxKind.FloatKeyword)),
                ScriptDataType.Int => PredefinedType(Token(SyntaxKind.IntKeyword)),
                ScriptDataType.Boolean => PredefinedType(Token(SyntaxKind.BoolKeyword)),
                ScriptDataType.Short => PredefinedType(Token(SyntaxKind.ShortKeyword)),
                ScriptDataType.String => PredefinedType(Token(SyntaxKind.StringKeyword)),
                
                _ => PredefinedType(Token(SyntaxKind.ObjectKeyword)),
            };
        }

        public static FieldDeclarationSyntax CreateFieldDeclaration(ScenarioTag.ScriptVariableDefinition variable, ExpressionSyntax rightHandSide)
        {
            return variable.DataType switch
            {
                ScriptDataType.Float => CreateDeclaration(SyntaxKind.FloatKeyword),
                ScriptDataType.Int => CreateDeclaration(SyntaxKind.IntKeyword),
                ScriptDataType.Boolean => CreateDeclaration(SyntaxKind.BoolKeyword),
                ScriptDataType.Short => CreateDeclaration(SyntaxKind.ShortKeyword),
                ScriptDataType.String => CreateDeclaration(SyntaxKind.StringKeyword),

                _ => throw new NotImplementedException(),
            };

            FieldDeclarationSyntax CreateDeclaration(SyntaxKind keyword)
            {
                return FieldDeclaration(VariableDeclaration(PredefinedType(Token(keyword)))
                    .WithVariables(SingletonSeparatedList<VariableDeclaratorSyntax>(
                        VariableDeclarator(variable.Description)
                        .WithInitializer(EqualsValueClause(rightHandSide)))));
            }
        }

        public static LiteralExpressionSyntax LiteralExpression(ScenarioTag tag, ScenarioTag.ScriptSyntaxNode node)
        {
            return node.DataType switch
            {
                ScriptDataType.Float => LiteralExpression(BitConverter.Int32BitsToSingle((int)node.NodeData_32)),
                ScriptDataType.Int => LiteralExpression((int)node.NodeData_32),
                ScriptDataType.Boolean=> LiteralExpression(node.NodeData_B3 == 1),
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
