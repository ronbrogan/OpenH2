using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenH2.Serialization.Materialization;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace OpenH2.Serialization
{
    internal static class SyntaxUtils
    {
        public static ForStatementSyntax ForLoop(StatementSyntax body, int start, int count, string i = "i")
        {
            return ForLoop(body,
                start,
                LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(count)),
                i);
        }

        public static ForStatementSyntax ForLoop(StatementSyntax body, int start, ExpressionSyntax count, string i = "i")
        {
            return ForStatement(body)
                    .WithDeclaration(VariableDeclaration(IdentifierName("var"))
                        .AddVariables(VariableDeclarator(Identifier(i))
                            .WithInitializer(EqualsValueClause(LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(start))))))
                    .WithCondition(BinaryExpression(SyntaxKind.LessThanExpression,
                        IdentifierName(i),
                        count))
                    .AddIncrementors(PostfixUnaryExpression(SyntaxKind.PostIncrementExpression, IdentifierName(i)));
        }

        public static LocalDeclarationStatementSyntax LocalVar(SyntaxToken identifier, ExpressionSyntax initializer)
        {
            return LocalDeclarationStatement(VariableDeclaration(IdentifierName("var")).AddVariables(
                VariableDeclarator(identifier).WithInitializer(EqualsValueClause(initializer))));
        }

        public static ExpressionSyntax ReadSpanInt32(SyntaxToken spanIdentifier, SyntaxToken startIdentifier, int offset)
        {
            return ReadSpanInt32(spanIdentifier, BinaryExpression(SyntaxKind.AddExpression,
                            IdentifierName(startIdentifier),
                            LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(offset))));
        }

        public static ExpressionSyntax ReadSpanInt32(SyntaxToken spanIdentifier, ExpressionSyntax start)
        {
            return InvocationExpression(
                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName(spanIdentifier),
                        IdentifierName(nameof(SpanByteExtensions.ReadInt32At))))
                    .AddArgumentListArguments(
                        Argument(start));
        }
    }
}
