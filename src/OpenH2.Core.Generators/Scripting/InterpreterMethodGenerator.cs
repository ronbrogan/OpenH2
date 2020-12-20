using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace OpenH2.Core.Generators.Scripting
{
    [Generator]
    public partial class InterpreterMethodGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
        }

        public void Execute(GeneratorExecutionContext context)
        {
            //Debugger.Launch();

            var scriptEngine = context.Compilation.GetTypeByMetadataName("OpenH2.Core.Scripting.IScriptEngine");

            var cls = GeneratePartialClass(scriptEngine);

            var ns = NamespaceDeclaration(ParseName("OpenH2.Core.Scripting.Execution"))
                            .AddUsings(
                                UsingDirective(ParseName("System")),
                                UsingDirective(ParseName("System.IO")),
                                UsingDirective(ParseName("System.Diagnostics")),
                                UsingDirective(ParseName("OpenH2.Core.GameObjects")),
                                UsingDirective(ParseName("OpenH2.Core.Tags")),
                                UsingDirective(ParseName("OpenH2.Core.Tags.Scenario")))
                            .AddMembers(cls);

            var source = ns.NormalizeWhitespace().ToString();

            var fileName = "ScriptInterpreter.GeneratedMethods.cs";

            context.AddSource(fileName, SourceText.From(source, Encoding.UTF8));

            try
            {
                File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), "obj", fileName), source);
            }
            catch { }
        }

        private ClassDeclarationSyntax GeneratePartialClass(INamedTypeSymbol engineInterface)
        {
            var className = "ScriptInterpreter";

            var interfaceMembers = engineInterface.GetMembers().OfType<IMethodSymbol>();

            var implMembers = new List<MemberDeclarationSyntax>();

            var implementations = new List<(int, string)>();

            foreach(var mem in interfaceMembers)
            {
                var attrs = mem.GetAttributes();

                var implAttr = attrs.FirstOrDefault(a => a.AttributeClass.Name == "ScriptImplementationAttribute");

                if (implAttr == null)
                    continue;

                var opCode = (int)implAttr.ConstructorArguments[0].Value;

                var body = new List<StatementSyntax>();

                body.Add(LocalDeclarationStatement(
                        VariableDeclaration(IdentifierName("var"))
                        .AddVariables(VariableDeclarator(Identifier("argNext"))
                            .WithInitializer(
                                EqualsValueClause(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                    IdentifierName("node"),
                                    IdentifierName("NextIndex")))))));

                // Interpret argument expressions
                foreach(var param in mem.Parameters)
                {
                    body.Add(LocalDeclarationStatement(VariableDeclaration(IdentifierName("var")).WithVariables(
                        SingletonSeparatedList(VariableDeclarator(Identifier(param.Name))
                            .WithInitializer(EqualsValueClause(
                                InvocationExpression(IdentifierName("Interpret"))
                                .WithArgumentList(ArgumentList(SeparatedList<ArgumentSyntax>(
                                    new SyntaxNodeOrToken[]{
                                        Argument(ElementAccessExpression(
                                            MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                    ThisExpression(),
                                                    IdentifierName("scenario")),
                                                IdentifierName("ScriptSyntaxNodes")))
                                            .WithArgumentList(BracketedArgumentList(
                                                SingletonSeparatedList(
                                                    Argument(IdentifierName("argNext")))))),
                                        Token(SyntaxKind.CommaToken),
                                        Argument(IdentifierName("argNext"))
                                            .WithRefKindKeyword(Token(SyntaxKind.OutKeyword))})))))))));
                }

                // Ensure that we're done with the available arguments
                body.Add(ExpressionStatement(InvocationExpression(
                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName("Debug"),
                        IdentifierName("Assert")))
                    .AddArgumentListArguments(
                        Argument(BinaryExpression(SyntaxKind.EqualsExpression,
                            IdentifierName("argNext"),
                            MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                PredefinedType(Token(SyntaxKind.UShortKeyword)),
                                IdentifierName("MaxValue")))))));

                // Invoke implementation with properly typed arguments
                var invoke = InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        ThisExpression(),
                        IdentifierName("scriptEngine")),
                    IdentifierName(mem.Name)));

                foreach (var param in mem.Parameters)
                {
                    invoke = invoke.AddArgumentListArguments(Argument(GetValueForParam(param)));
                }

                if(mem.ReturnType.SpecialType == SpecialType.System_Void)
                {
                    body.Add(ExpressionStatement(invoke));
                }
                else
                {
                    body.Add(LocalDeclarationStatement(
                        VariableDeclaration(IdentifierName("var"))
                        .AddVariables(VariableDeclarator(Identifier("result"))
                            .WithInitializer(
                                EqualsValueClause(invoke)))));
                }

                body.Add(WrapReturnValue(mem.ReturnType));

                var implName = "Invoke_" + mem.Name + "_" + opCode;

                implementations.Add((opCode, implName));

                implMembers.Add(MethodDeclaration(ParseTypeName("InterpreterResult"), implName)
                    .WithBody(Block(body))
                    .AddParameterListParameters(
                        Parameter(Identifier("node"))
                        .WithType(QualifiedName(
                            IdentifierName("ScenarioTag"),
                            IdentifierName("ScriptSyntaxNode")))));
            }

            implMembers.Add(CreateDispatchImplementation(implementations));

            var cls = ClassDeclaration(className).WithModifiers(TokenList(Token(SyntaxKind.PartialKeyword)))
                .WithMembers(List(implMembers));

            return cls;
        }

        MethodDeclarationSyntax CreateDispatchImplementation(List<(int, string)> implementations)
        {
            var arms = new List<SwitchExpressionArmSyntax>();

            foreach(var (i,n) in implementations)
            {
                arms.Add(SwitchExpressionArm(
                    ConstantPattern(LiteralExpression(SyntaxKind.NumericLiteralExpression,
                        Literal(i))),
                    InvocationExpression(
                        MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                            ThisExpression(),
                            IdentifierName(n)))
                    .AddArgumentListArguments(Argument(IdentifierName("node")))));
            }

            arms.Add(SwitchExpressionArm(
                DiscardPattern(),
                ThrowExpression(
                    ObjectCreationExpression(
                        IdentifierName("NotImplementedException")).WithArgumentList(ArgumentList()))));

            var switchExp = SwitchExpression(
                MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName("node"),
                    IdentifierName("OperationId")))
                .AddArms(arms.ToArray());

            var decl = MethodDeclaration(ParseTypeName("InterpreterResult"), "DispatchMethod")
                    .AddModifiers(Token(SyntaxKind.PrivateKeyword), Token(SyntaxKind.PartialKeyword))
                    .WithBody(Block(ReturnStatement(switchExp)))
                    .AddParameterListParameters(
                        Parameter(Identifier("node"))
                        .WithType(QualifiedName(
                            IdentifierName("ScenarioTag"),
                            IdentifierName("ScriptSyntaxNode"))));

            return decl;
        }

        ExpressionSyntax GetValueForParam(IParameterSymbol param)
        {
            if (param.Type.SpecialType == SpecialType.System_Int16)
            {
                return MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName(param.Name),
                    IdentifierName("Short"));
            }
            else if (param.Type.SpecialType == SpecialType.System_Int32)
            {
                return MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName(param.Name),
                    IdentifierName("Int"));
            }
            else if (param.Type.SpecialType == SpecialType.System_Single)
            {
                return MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName(param.Name),
                    IdentifierName("Float"));
            }
            else if (param.Type.SpecialType == SpecialType.System_Boolean)
            {
                return MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName(param.Name),
                    IdentifierName("Boolean"));
            }
            else
            {
                return CastExpression(ParseTypeName(param.Type.Name), MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    IdentifierName(param.Name),
                    IdentifierName("Object")));
            }
        }

        StatementSyntax WrapReturnValue(ITypeSymbol returnType)
        {
            var val = InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                IdentifierName("InterpreterResult"),
                IdentifierName("From")));

            if (returnType.SpecialType == SpecialType.System_Int16)
            {
                val = val.AddArgumentListArguments(Argument(IdentifierName("result")));
            }
            else if (returnType.SpecialType == SpecialType.System_Int32)
            {
                val = val.AddArgumentListArguments(Argument(IdentifierName("result")));
            }
            else if (returnType.SpecialType == SpecialType.System_Single)
            {
                val = val.AddArgumentListArguments(Argument(IdentifierName("result")));
            }
            else if (returnType.SpecialType == SpecialType.System_Boolean)
            {
                val = val.AddArgumentListArguments(Argument(IdentifierName("result")));
            }
            else if (TypeMapping.ImplementationToScriptDataType.TryGetValue(returnType.Name.Split('.').Last(), out var sdt))
            {
                val = val.AddArgumentListArguments(
                        Argument(IdentifierName("result")),
                        Argument(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName("ScriptDataType"),
                            IdentifierName(sdt))));
            }

            return ReturnStatement(val);
        }

        private INamedTypeSymbol GetType(Compilation compilation, string typeName)
        {
            var assemblies = compilation.References.Select(compilation.GetAssemblyOrModuleSymbol)
                .OfType<IAssemblySymbol>()
                .ToList();

            return assemblies
                .Select(a => a.GetTypeByMetadataName(typeName))
                .Single(a => a != null);
        }
    }


}
