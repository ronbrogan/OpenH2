using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenH2.Core.Extensions;
using OpenH2.Core.Scripting;
using OpenH2.Core.Tags.Scenario;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace OpenH2.ScriptAnalysis
{
    public static class ScriptGenAnnotations
    {
        public static SyntaxAnnotation ResultStatement { get; } = new SyntaxAnnotation("ResultStatement");
    }

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
                
                _ => Enum.IsDefined(typeof(ScriptDataType), dataType) 
                ? ParseTypeName(dataType.ToString())
                : ParseTypeName(nameof(ScriptDataType) + dataType.ToString()),
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
            return FieldDeclaration(VariableDeclaration(ScriptTypeSyntax(variable.DataType))
                    .WithVariables(SingletonSeparatedList<VariableDeclaratorSyntax>(
                        VariableDeclarator(SanitizeIdentifier(variable.Description))
                        .WithInitializer(EqualsValueClause(rightHandSide)))));
        }

        private static HashSet<ScriptDataType> stringLiteralTypes = new HashSet<ScriptDataType>()
        {
            { ScriptDataType.String },
            { ScriptDataType.ReferenceGet },
            { ScriptDataType.Animation },
            { ScriptDataType.Weapon },
            { ScriptDataType.SpatialPoint },
            { ScriptDataType.WeaponReference }
        };

        public static LiteralExpressionSyntax LiteralExpression(ScenarioTag tag, ScenarioTag.ScriptSyntaxNode node)
        {
            if(stringLiteralTypes.Contains(node.DataType))
            {
                return LiteralExpression(GetScriptString(tag, node));
            }

            return node.DataType switch
            {
                ScriptDataType.Float => LiteralExpression(BitConverter.Int32BitsToSingle((int)node.NodeData_32)),
                ScriptDataType.Int => LiteralExpression((int)node.NodeData_32),
                ScriptDataType.Boolean=> LiteralExpression(node.NodeData_B3 == 1),
                ScriptDataType.Short => LiteralExpression(node.NodeData_H16),

                _ => throw new NotImplementedException(),
            };
        }

        public static string GetScriptString(ScenarioTag tag, ScenarioTag.ScriptSyntaxNode node)
        {
            return ((Span<byte>)tag.ScriptStrings).ReadStringStarting(node.NodeString);
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

        public static bool TryGetContainingSimpleExpression(this StatementSyntax statement, out ExpressionSyntax simple)
        {
            if (statement is ReturnStatementSyntax ret)
            {
                simple = ret.Expression.IsSimpleExpression() ? ret.Expression : default;
            }
            else if (statement is ExpressionStatementSyntax exp)
            {
                simple = exp.Expression.IsSimpleExpression() ? exp.Expression : default;
            }
            else
            {
                simple = default;
            }

            return simple != default;
        }

        private static Type[] simpleExpressionTypes = new Type[]
        {
            typeof(IdentifierNameSyntax),
            typeof(LiteralExpressionSyntax),
            typeof(InvocationExpressionSyntax),
            typeof(MemberAccessExpressionSyntax),
            typeof(ParenthesizedExpressionSyntax),
            typeof(BinaryExpressionSyntax),
            typeof(PrefixUnaryExpressionSyntax),
            typeof(PostfixUnaryExpressionSyntax),
        };

        public static bool IsSimpleExpression(this ExpressionSyntax exp)
        {
            return Array.IndexOf(simpleExpressionTypes, exp.GetType()) >= 0;
        }

        public static bool TryGetLeftHandExpression(this StatementSyntax statement, out ExpressionSyntax rhs)
        {
            rhs = statement switch
            {
                ExpressionStatementSyntax exp => HandleExpression(exp.Expression),
                ReturnStatementSyntax ret => HandleExpression(ret.Expression),

                _ => default
            };

            return rhs != default;

            ExpressionSyntax HandleExpression(ExpressionSyntax exp)
            {
                return exp switch
                {
                    AssignmentExpressionSyntax ass => ass.Left,

                    _ => default
                };
            }
        }

        public static ExpressionSyntax CreateImmediatelyInvokedFunction(ScriptDataType returnType, IEnumerable<StatementSyntax> body)
        {
            ObjectCreationExpressionSyntax funcObj;

            if (returnType == ScriptDataType.Void)
            {
                funcObj = SyntaxFactory.ObjectCreationExpression(
                    SyntaxFactory.IdentifierName("Action"));
            }
            else
            {
                funcObj = SyntaxFactory.ObjectCreationExpression(
                    SyntaxFactory.GenericName("Func")
                        .AddTypeArgumentListArguments(SyntaxUtil.ScriptTypeSyntax(returnType)));
            }

            return InvocationExpression(
                funcObj.AddArgumentListArguments(
                    Argument(
                        ParenthesizedLambdaExpression(
                            Block(
                                List(body))))));
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
