using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenH2.Core.Extensions;
using OpenH2.Core.GameObjects;
using OpenH2.Core.Tags.Scenario;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace OpenH2.Core.Scripting.Generation
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
                ScriptDataType.Short => PredefinedType(Token(SyntaxKind.IntKeyword)),
                ScriptDataType.String => PredefinedType(Token(SyntaxKind.StringKeyword)),
                ScriptDataType.Void => PredefinedType(Token(SyntaxKind.VoidKeyword)),

                ScriptDataType.Trigger => ParseTypeName(nameof(ITriggerVolume)),
                ScriptDataType.LocationFlag => ParseTypeName(nameof(ILocationFlag)),
                ScriptDataType.CameraPathTarget => ParseTypeName(nameof(ICameraPathTarget)),
                ScriptDataType.CinematicTitle => ParseTypeName(nameof(ICinematicTitle)),
                ScriptDataType.DeviceGroup => ParseTypeName(nameof(IDeviceGroup)),
                ScriptDataType.AIScript => ParseTypeName(nameof(IAiScript)),
                ScriptDataType.AIOrders => ParseTypeName(nameof(IAiOrders)),
                ScriptDataType.Equipment => ParseTypeName(nameof(IEquipment)),
                ScriptDataType.Weapon => ParseTypeName(nameof(IWeapon)),
                ScriptDataType.Scenery => ParseTypeName(nameof(IScenery)),

                ScriptDataType.Bloc => ParseTypeName(nameof(IBloc)),
                ScriptDataType.Unit => ParseTypeName(nameof(IUnit)),
                ScriptDataType.Vehicle => ParseTypeName(nameof(IVehicle)),
                ScriptDataType.AI => ParseTypeName(nameof(IAiActor)),
                ScriptDataType.Device => ParseTypeName(nameof(IDevice)),

                _ => Enum.IsDefined(typeof(ScriptDataType), dataType)
                    ? ParseTypeName("I" + dataType.ToString())
                    : ParseTypeName(nameof(ScriptDataType) + dataType.ToString()),
            };
        }

        public static ScriptDataType ToScriptDataType(ScenarioTag.WellKnownVarType type)
        {
            return type switch
            {
                ScenarioTag.WellKnownVarType.Biped => ScriptDataType.Unit,
                ScenarioTag.WellKnownVarType.Vehicle => ScriptDataType.Vehicle,
                ScenarioTag.WellKnownVarType.Weapon => ScriptDataType.Weapon,
                ScenarioTag.WellKnownVarType.Equipment => ScriptDataType.Equipment,
                ScenarioTag.WellKnownVarType.Scenery => ScriptDataType.Scenery,
                ScenarioTag.WellKnownVarType.Machinery => ScriptDataType.Device,
                ScenarioTag.WellKnownVarType.Controller => ScriptDataType.Device,
                ScenarioTag.WellKnownVarType.Sound => ScriptDataType.LoopingSound,
                ScenarioTag.WellKnownVarType.Bloc => ScriptDataType.Bloc,
                _ => throw new NotSupportedException()
            };
        }

        public static TypeSyntax TypeSyntax(Type dataType)
        {
            if(typeSyntaxCreators.TryGetValue(dataType, out var func))
            {
                return func();
            }

            return ParseTypeName(dataType.ToString());
        }

        private static Dictionary<Type, Func<TypeSyntax>> typeSyntaxCreators = new Dictionary<Type, Func<TypeSyntax>>()
        {
            { typeof(int), () => PredefinedType(Token(SyntaxKind.IntKeyword)) },
            { typeof(short),() => PredefinedType(Token(SyntaxKind.ShortKeyword)) },
            { typeof(float), () => PredefinedType(Token(SyntaxKind.FloatKeyword)) },
            { typeof(string), () => PredefinedType(Token(SyntaxKind.StringKeyword)) },
            { typeof(bool), () => PredefinedType(Token(SyntaxKind.BoolKeyword)) },
        };

        private static Dictionary<Type, ScriptDataType> toScriptTypeMap = new Dictionary<Type, ScriptDataType>()
        {
            { typeof(int), ScriptDataType.Int },
            { typeof(short), ScriptDataType.Short },
            { typeof(float), ScriptDataType.Float },
            { typeof(string), ScriptDataType.String },
            { typeof(bool), ScriptDataType.Boolean },
            { typeof(ITeam), ScriptDataType.Team },
            { typeof(IAiActor), ScriptDataType.AI },
            { typeof(IAIBehavior), ScriptDataType.AIBehavior },
            { typeof(IDamageState), ScriptDataType.DamageState },
            { typeof(INavigationPoint), ScriptDataType.NavigationPoint }
        };

        private static Dictionary<ScriptDataType, Type> toTypeMap = toScriptTypeMap.ToDictionary(kv => kv.Value, kv => kv.Key);

        public static bool TryGetScriptTypeFromType(Type t, out ScriptDataType scriptType)
        {
            return toScriptTypeMap.TryGetValue(t, out scriptType);
        }

        public static bool TryGetTypeFromScriptType(ScriptDataType s, out Type t)
        {
            return toTypeMap.TryGetValue(s, out t);
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

            if (char.IsDigit(name[0]) || Array.IndexOf(_keywords, name) >= 0)
            {
                name = "_" + name;
            }

            return name;
        }

        public static string SanitizeMemberAccess(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return name;
            }

            var segments = name.Split('.');

            for (var i = 0; i < segments.Length; i++)
            {
                segments[i] = SanitizeIdentifier(segments[i]);
            }

            return string.Join(".", segments);
        }

        public static FieldDeclarationSyntax CreateField(ScenarioTag.ScriptVariableDefinition variable)
        {
            return FieldDeclaration(VariableDeclaration(ScriptTypeSyntax(variable.DataType))
                    .WithVariables(SingletonSeparatedList<VariableDeclaratorSyntax>(
                        VariableDeclarator(SanitizeIdentifier(variable.Description)))))
                .WithAdditionalAnnotations(ScriptGenAnnotations.TypeAnnotation(variable.DataType));
        }

        public static PropertyDeclarationSyntax CreateProperty(ScriptDataType type, string name)
        {
            return PropertyDeclaration(ScriptTypeSyntax(type), name)
                    .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                    .WithAccessorList(AutoPropertyAccessorList())
                    .WithAdditionalAnnotations(ScriptGenAnnotations.TypeAnnotation(type));
        }

        public static PropertyDeclarationSyntax CreateProperty(string type, string name)
        {
            return PropertyDeclaration(ParseTypeName(type), name)
                    .WithModifiers(TokenList(Token(SyntaxKind.PublicKeyword)))
                    .WithAccessorList(AutoPropertyAccessorList());
        }

        public static IReadOnlyList<ScriptDataType> StringLiteralTypes = new ScriptDataType[]
        {
            ScriptDataType.String,
            ScriptDataType.StringId,
            ScriptDataType.ReferenceGet,
            ScriptDataType.Animation,
            ScriptDataType.Weapon,
            ScriptDataType.SpatialPoint,
            ScriptDataType.WeaponReference,
            ScriptDataType.GameDifficulty,
            ScriptDataType.VehicleSeat
        };

        public static IReadOnlyList<ScriptDataType> NumericLiteralTypes = new ScriptDataType[]
        {
            ScriptDataType.Float,
            ScriptDataType.Int,
            ScriptDataType.Short
        };

        public static LiteralExpressionSyntax LiteralExpression(ScenarioTag tag, ScenarioTag.ScriptSyntaxNode node, ScriptDataType destinationType)
        {
            if (StringLiteralTypes.Contains(node.DataType))
            {
                return LiteralExpression(GetScriptString(tag, node))
                    .WithAdditionalAnnotations(ScriptGenAnnotations.TypeAnnotation(destinationType));
            }

            object nodeValue = node.DataType switch
            {
                // TODO: netstandard2.1 or net5 upgrade, replace with BitConverter.Int32BitsToSingle
                ScriptDataType.Float => BitConverter.ToSingle(BitConverter.GetBytes((int)node.NodeData_32), 0),
                ScriptDataType.Int => (int)node.NodeData_32,
                ScriptDataType.Boolean => node.NodeData_B3 == 1,
                ScriptDataType.Short => (short)node.NodeData_H16,

                _ => throw new NotImplementedException(),
            };


            var exp = destinationType switch
            {
                ScriptDataType.Float => LiteralExpression(Convert.ToSingle(nodeValue)),
                ScriptDataType.Int => LiteralExpression(Convert.ToInt32(nodeValue)),
                ScriptDataType.Boolean => LiteralExpression(Convert.ToBoolean(nodeValue)),
                ScriptDataType.Short => LiteralExpression(Convert.ToInt16(nodeValue)),

                _ => throw new NotImplementedException(),
            };

            return exp.WithAdditionalAnnotations(ScriptGenAnnotations.TypeAnnotation(destinationType));
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
            typeof(AwaitExpressionSyntax),
            typeof(CastExpressionSyntax),
            typeof(IdentifierNameSyntax),
            typeof(LiteralExpressionSyntax),
            typeof(InvocationExpressionSyntax),
            typeof(MemberAccessExpressionSyntax),
            typeof(ParenthesizedExpressionSyntax),
            typeof(BinaryExpressionSyntax),
            typeof(PrefixUnaryExpressionSyntax),
            typeof(PostfixUnaryExpressionSyntax),
            typeof(DefaultExpressionSyntax)
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

        public static ExpressionSyntax CreateCast(Type from, Type to, ExpressionSyntax inner)
        {
            if(to == typeof(bool) && NumericTypes.Contains(from))
            {
                // Generate comparison conditional result
                return ConditionalExpression(
                    BinaryExpression(SyntaxKind.EqualsExpression,
                        inner,
                        Token(SyntaxKind.EqualsEqualsToken),
                        LiteralExpression(1)),
                    LiteralExpression(true),
                    LiteralExpression(false));
            }
            else if (NumericTypes.Contains(to) && from == typeof(bool))
            {
                return ConditionalExpression(inner, LiteralExpression(1), LiteralExpression(0));
            }
            else
            {
                return CastExpression(TypeSyntax(to), inner);
            }
        }

        public static ExpressionSyntax CreateCast(ScriptDataType from, ScriptDataType to, ExpressionSyntax inner)
        {
            if(toTypeMap.TryGetValue(from, out var fromT) &&
                toTypeMap.TryGetValue(to, out var toT))
            {
                return CreateCast(fromT, toT, inner).WithAdditionalAnnotations(ScriptGenAnnotations.TypeAnnotation(to));
            }
            else
            {
                return inner.WithTrailingTrivia(Comment($"// Couldn't generate cast from '{from}' to '{to}'"));
            }
        }

        private static readonly HashSet<Type> NumericTypes = new HashSet<Type>
        {
            typeof(int),  typeof(double),  typeof(decimal),
            typeof(long), typeof(short),   typeof(sbyte),
            typeof(byte), typeof(ulong),   typeof(ushort),
            typeof(uint), typeof(float)
        };

        public static ScriptDataType BinaryNumericPromotion(ScriptDataType a, ScriptDataType b)
        {
            Debug.Assert(NumericLiteralTypes.Contains(a) && NumericLiteralTypes.Contains(b));

            if (a == ScriptDataType.Float || b == ScriptDataType.Float)
                return ScriptDataType.Float;
            else
                return ScriptDataType.Int;
        }

        public static bool TryGetTypeOfExpression(ExpressionSyntax exp, out ScriptDataType type)
        {
            var ext = new TypeExtractor();
            ext.Visit(exp);

            type = ext.Type ?? default;
            return ext.Type != default;
        }

        private class TypeExtractor : CSharpSyntaxWalker
        {
            public ScriptDataType? Type { get; private set; } = null;


            public override void Visit(SyntaxNode node)
            {
                if(node.HasAnnotations(ScriptGenAnnotations.TypeAnnotationKind))
                {
                    var types = node.GetAnnotations(ScriptGenAnnotations.TypeAnnotationKind);

                    this.Type = (ScriptDataType)int.Parse(types.First().Data);
                    return;
                }

                base.Visit(node);
            }
        }

        public static ExpressionSyntax CreateImmediatelyInvokedFunction(ScriptDataType returnType, IEnumerable<StatementSyntax> body)
        {
            ObjectCreationExpressionSyntax funcObj;

            if (returnType == ScriptDataType.Void)
            {
                funcObj = ObjectCreationExpression(IdentifierName("Action"));
            }
            else
            {
                funcObj = ObjectCreationExpression(GenericName("Func")
                        .AddTypeArgumentListArguments(ScriptTypeSyntax(returnType)));
            }

            return InvocationExpression(
                funcObj.AddArgumentListArguments(
                    Argument(ParenthesizedLambdaExpression(Block(List(body)))
                        .WithAsyncKeyword(Token(SyntaxKind.AsyncKeyword)))))
                .WithAdditionalAnnotations(ScriptGenAnnotations.TypeAnnotation(returnType));
        }

        public static AccessorListSyntax AutoPropertyAccessorList()
        {
            return AccessorList(List(new[] {
                AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                    .WithSemicolonToken(Token(TriviaList(), SyntaxKind.SemicolonToken, TriviaList(Space))),
                AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                    .WithSemicolonToken(Token(TriviaList(), SyntaxKind.SemicolonToken, TriviaList(Space)))
            }));
        }

        public static CSharpSyntaxNode Normalize(CSharpSyntaxNode node)
        {
            node = node.NormalizeWhitespace();

            var norm = new DeNormalizer();
            var newNode = norm.Visit(node);


            return newNode as CSharpSyntaxNode;
        }

        public static bool AwaitIfNeeded(MethodInfo method, ref ExpressionSyntax invocation, out Type innerType)
        {
            innerType = method.ReturnType;

            if (method.ReturnType == typeof(Task) ||
                (method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>)))
            {
                invocation = SyntaxFactory.AwaitExpression(invocation);

                if (method.ReturnType == typeof(Task))
                {
                    innerType = typeof(void);
                }
                else
                {
                    innerType = method.ReturnType.GetGenericArguments()[0];
                }

                return true;
            }

            return false;
        }

        public delegate bool ResultVarGenerator(ExpressionSyntax exp, out StatementSyntax statement);
        public static void CreateReturnStatement(ScriptDataType returnType, List<StatementSyntax> statements, ResultVarGenerator resultGen)
        {
            if(statements.Any() == false)
            {
                return;
            }

            var last = statements.Last();

            if (returnType == ScriptDataType.Void)
            {
                statements.Remove(last);
                statements.Add(last.WithAdditionalAnnotations(ScriptGenAnnotations.FinalScopeStatement));
                return;
            }

            if (last.TryGetContainingSimpleExpression(out var lastExp) && resultGen(lastExp, out var lastStatement))
            {
                statements.Remove(last);
                statements.Add(lastStatement.WithAdditionalAnnotations(ScriptGenAnnotations.FinalScopeStatement));
            }
            else if (last.TryGetLeftHandExpression(out var lhsExp) && resultGen(lhsExp, out var lhsStatement))
            {
                statements.Add(lhsStatement.WithAdditionalAnnotations(ScriptGenAnnotations.FinalScopeStatement));
            }
            else
            {
                var defaultExp = SyntaxFactory.LiteralExpression(
                        SyntaxKind.DefaultLiteralExpression,
                        SyntaxFactory.Token(SyntaxKind.DefaultKeyword));

                if (resultGen(defaultExp, out var defaultStatement))
                {
                    statements.Add(defaultStatement.WithAdditionalAnnotations(ScriptGenAnnotations.FinalScopeStatement)
                        .WithTrailingTrivia(SyntaxFactory.Comment("// Unhandled 'begin' return")));
                }
            }
        }

        public static bool HasReturnStatement(IEnumerable<SyntaxNode> roots)
        {
            foreach (var root in roots)
            {
                var checker = new ReturnChecker();

                checker.Visit(root);

                if (checker.HasReturn)
                    return true;
            }

            return false;
        }

        private class ReturnChecker : CSharpSyntaxWalker
        {
            public bool HasReturn { get; private set; } = false;

            public override void VisitReturnStatement(ReturnStatementSyntax node)
            {
                HasReturn = true;

                base.VisitReturnStatement(node);
            }

            public override void VisitParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax node)
            {
            }
        }

        public class DeNormalizer : CSharpSyntaxRewriter
        {
            private AccessorListSyntax autoPropList = AccessorList(List<AccessorDeclarationSyntax>(new[] {
                AccessorDeclaration(SyntaxKind.GetAccessorDeclaration)
                    .WithSemicolonToken(Token(TriviaList(), SyntaxKind.SemicolonToken, TriviaList(Space))),
                AccessorDeclaration(SyntaxKind.SetAccessorDeclaration)
                    .WithSemicolonToken(Token(TriviaList(), SyntaxKind.SemicolonToken, TriviaList(Space)))
            }));

            public override SyntaxNode? VisitPropertyDeclaration(PropertyDeclarationSyntax node)
            {
                if (AreEquivalent(node.AccessorList, autoPropList, false))
                {
                    var newNode = node
                        .WithIdentifier(node.Identifier.WithTrailingTrivia(Space))
                        .WithAccessorList(autoPropList.WithOpenBraceToken(
                            Token(
                                TriviaList(),
                                SyntaxKind.OpenBraceToken,
                                TriviaList(
                                    Space)))
                            .WithCloseBraceToken(
                                Token(
                                    TriviaList(),
                                    SyntaxKind.CloseBraceToken,
                                    TriviaList(
                                        LineFeed))));


                    return base.VisitPropertyDeclaration(newNode);
                }

                return base.VisitPropertyDeclaration(node);
            }

            public override SyntaxNode VisitArgumentList(ArgumentListSyntax node)
            {
                var newArgs = node.Arguments;

                if(newArgs.Count > 1)
                {
                    for (var i = 0; i < node.Arguments.Count - 1; i++)
                    {
                        newArgs = newArgs.ReplaceSeparator(newArgs.GetSeparator(i), Token(SyntaxKind.CommaToken).WithTrailingTrivia(Space));
                    }
                }

                var newLineAndIndent = TriviaList(CarriageReturnLineFeed)
                    .AddRange(node.Parent.GetLeadingTrivia())
                    .Add(SyntaxFactory.Space)
                    .Add(SyntaxFactory.Space)
                    .Add(SyntaxFactory.Space)
                    .Add(SyntaxFactory.Space);

                for(var i = 1; i < newArgs.Count; i++)
                {
                    var arg = newArgs[i];
                    if(arg.Expression is ParenthesizedLambdaExpressionSyntax lambda)
                    {
                        newArgs = newArgs.Replace(arg, arg.WithExpression(lambda.WithLeadingTrivia(newLineAndIndent)));
                    }
                }

                node = node.WithCloseParenToken(node.CloseParenToken.WithLeadingTrivia())
                    .WithArguments(newArgs);

                return base.VisitArgumentList(node);
            }

            public override SyntaxNode VisitParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax node)
            {
                var newNode = node;

                if(newNode.Body is BlockSyntax block)
                {
                    newNode = newNode.WithBody(block.WithCloseBraceToken(block.CloseBraceToken.WithTrailingTrivia()));
                }

                return base.VisitParenthesizedLambdaExpression(newNode);
            }

            public override SyntaxNode VisitInitializerExpression(InitializerExpressionSyntax node)
            {
                var newlined = new List<ExpressionSyntax>();

                var leading = TriviaList(CarriageReturnLineFeed)
                    .AddRange(node.GetLeadingTrivia())
                    .Add(SyntaxFactory.Space)
                    .Add(SyntaxFactory.Space)
                    .Add(SyntaxFactory.Space)
                    .Add(SyntaxFactory.Space);

                ExpressionSyntax last = null;
                foreach(var exp in node.Expressions)
                {
                    last = exp.WithLeadingTrivia(leading);
                    newlined.Add(last);
                }

                if(last != null)
                {
                    newlined[^1] = last.WithTrailingTrivia(TriviaList(CarriageReturnLineFeed)
                    .AddRange(node.GetLeadingTrivia()));
                }

                return base.VisitInitializerExpression(node.WithExpressions(SeparatedList(newlined)));
            }
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
