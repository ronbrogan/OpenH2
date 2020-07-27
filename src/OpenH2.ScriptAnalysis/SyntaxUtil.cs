using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenH2.Core.Extensions;
using OpenH2.Core.Scripting;
using OpenH2.Core.Tags.Scenario;
using System;
using System.Linq;
using System.Text.RegularExpressions;
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
                ScriptDataType.Void => PredefinedType(Token(SyntaxKind.VoidKeyword)),
                
                _ => PredefinedType(Token(SyntaxKind.ObjectKeyword)).WithTrailingTrivia(Comment($"/*{dataType}*/")),
            };
        }

        private static Regex IdentifierInvalidChars = new Regex("[^\\dA-Za-z]", RegexOptions.Compiled);
        public static string SanitizeIdentifier(string name)
        {
            if(string.IsNullOrEmpty(name))
            {
                return name;
            }

            // BUGBUG: identifiers that only differ on separator characters would collide after this
            name = IdentifierInvalidChars.Replace(name, "_");

            if (char.IsDigit(name[0]))
            {
                name = "_" + name;
            }

            if(Array.IndexOf(_keywords, name) >= 0)
            {
                name = "@" + name;
            }

            return name;
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

                _ => CreateDeclaration(SyntaxKind.ObjectKeyword).WithTrailingTrivia(Comment("// Unhandled Type: " + variable.DataType))
            };

            FieldDeclarationSyntax CreateDeclaration(SyntaxKind keyword)
            {
                return FieldDeclaration(VariableDeclaration(PredefinedType(Token(keyword)))
                    .WithVariables(SingletonSeparatedList<VariableDeclaratorSyntax>(
                        VariableDeclarator(SanitizeIdentifier(variable.Description))
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

        private static readonly string[] _keywords = new[]
        {
            "abstract",  "event",      "new",        "struct",
            "as",        "explicit",   "null",       "switch",
            "base",      "extern",     "object",     "this",
            "bool",      "false",      "operator",   "throw",
            "break",     "finally",    "out",        "true",
            "byte",      "fixed",      "override",   "try",
            "case",      "float",      "params",     "typeof",
            "catch",     "for",        "private",    "uint",
            "char",      "foreach",    "protected",  "ulong",
            "checked",   "goto",       "public",     "unchekeced",
            "class",     "if",         "readonly",   "unsafe",
            "const",     "implicit",   "ref",        "ushort",
            "continue",  "in",         "return",     "using",
            "decimal",   "int",        "sbyte",      "virtual",
            "default",   "interface",  "sealed",     "volatile",
            "delegate",  "internal",   "short",      "void",
            "do",        "is",         "sizeof",     "while",
            "double",    "lock",       "stackalloc",
            "else",      "long",       "static",
            "enum",      "namespace",  "string",
            // contextual keywords 
            "add",       "alias",       "ascending",  "async",
            "await",      "by",         "descending", "dynamic",
            "equals",    "from",       "get",        "global",
            "group",      "into",       "join",       "let",
            "nameof",    "on",         "orderby",    "partial",
            "remove",    "select",     "set",        "value",
            "var",       "when",       "where",      "yield"
        };
    }
}
