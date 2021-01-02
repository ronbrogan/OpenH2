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
    public partial class IterativeInterpreterMethodGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
        }

        public void Execute(GeneratorExecutionContext context)
        {
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

            var fileName = "ScriptIterativeInterpreter.GeneratedMethods.cs";

            context.AddSource(fileName, SourceText.From(source, Encoding.UTF8));

            try
            {
                File.WriteAllText(Path.Combine(Directory.GetCurrentDirectory(), "obj", fileName), source);
            }
            catch { }
        }

        private ClassDeclarationSyntax GeneratePartialClass(INamedTypeSymbol engineInterface)
        {
            var className = "ScriptIterativeInterpreter";

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

                // Dequeue argument expressions from TopFrame's locals
                foreach(var param in mem.Parameters)
                {
                    body.Add(LocalDeclarationStatement(VariableDeclaration(IdentifierName("var")).WithVariables(
                        SingletonSeparatedList(VariableDeclarator(Identifier(param.Name))
                            .WithInitializer(EqualsValueClause(
                                InvocationExpression(
                                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                        MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                            MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                                IdentifierName("state"),
                                                IdentifierName("TopFrame")),
                                            IdentifierName("Locals")),
                                        IdentifierName("Dequeue")))))))));
                }

                // Ensure that we're done with the available arguments
                body.Add(ExpressionStatement(
                    InvocationExpression(MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        IdentifierName("Debug"),
                        IdentifierName("Assert")))
                    .AddArgumentListArguments(
                        Argument(
                            BinaryExpression(SyntaxKind.EqualsExpression,
                                MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                        MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                            IdentifierName("state"),
                                            IdentifierName("TopFrame")),
                                        IdentifierName("Locals")),
                                    IdentifierName("Count")),
                                LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(0)))))));

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

                body.Add(CreateInterpreterResult(mem.ReturnType));

                var implName = "Invoke_" + mem.Name + "_" + opCode;

                implementations.Add((opCode, implName));

                implMembers.Add(MethodDeclaration(PredefinedType(Token(SyntaxKind.VoidKeyword)), implName)
                    .WithBody(Block(body))
                    .AddParameterListParameters(
                        Parameter(Identifier("state"))
                            .WithModifiers(TokenList(Token(SyntaxKind.RefKeyword)))
                            .WithType(IdentifierName("InterpreterState"))));
            }

            implMembers.Add(CreateDispatchImplementation(implementations.OrderBy(i => i.Item1).ToList()));

            var cls = ClassDeclaration(className).WithModifiers(TokenList(Token(SyntaxKind.PartialKeyword)))
                .WithMembers(List(implMembers));

            return cls;
        }

        MethodDeclarationSyntax CreateDispatchImplementation(List<(int, string)> implementations)
        {
            var sections = new List<SwitchSectionSyntax>();

            foreach(var (i,n) in implementations)
            {
                sections.Add(SwitchSection()
                    .AddLabels(
                        CaseSwitchLabel(LiteralExpression(SyntaxKind.NumericLiteralExpression, Literal(i))))
                    .AddStatements(
                        ExpressionStatement(
                            InvocationExpression(
                                MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                                    ThisExpression(),
                                    IdentifierName(n)))
                            .AddArgumentListArguments(Argument(null, Token(SyntaxKind.RefKeyword), IdentifierName("state")))),
                        ReturnStatement()));
            }

            sections.Add(SwitchSection()
                .AddLabels(DefaultSwitchLabel())
                .AddStatements(
                    ThrowStatement(
                        ObjectCreationExpression(
                            IdentifierName("NotImplementedException")).WithArgumentList(ArgumentList()))));

            var switchStatement = SwitchStatement(
                MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                    MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                        MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression,
                            IdentifierName("state"),
                            IdentifierName("TopFrame")),
                        IdentifierName("OriginatingNode")),
                    IdentifierName("OperationId")))
                .AddSections(sections.ToArray());

            var decl = MethodDeclaration(PredefinedType(Token(SyntaxKind.VoidKeyword)), "DispatchMethod")
                    .AddModifiers(Token(SyntaxKind.PrivateKeyword), Token(SyntaxKind.PartialKeyword))
                    .WithBody(Block(switchStatement))
                    .AddParameterListParameters(
                        Parameter(Identifier("state"))
                            .WithModifiers(TokenList(Token(SyntaxKind.RefKeyword)))
                            .WithType(IdentifierName("InterpreterState")));

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

        StatementSyntax CreateInterpreterResult(ITypeSymbol returnType)
        {
            var completeFrame = InvocationExpression(IdentifierName("CompleteFrame"));
            var refStateArg = Argument(null, refKindKeyword: Token(SyntaxKind.RefKeyword), IdentifierName("state"));

            if (returnType.SpecialType == SpecialType.System_Void)
            {
                return ExpressionStatement(completeFrame.AddArgumentListArguments(refStateArg));
            }

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

            return ExpressionStatement(completeFrame.AddArgumentListArguments(
                Argument(val),
                refStateArg));
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
