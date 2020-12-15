using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Linq;
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

            var scriptEngine = GetType(context.Compilation, "OpenH2.Core.Scripting.IScriptEngine");


            var memebers = scriptEngine.GetMembers().OfType<IMethodSymbol>();

            var cls = GeneratePartialClass(scriptEngine);

            var ns = NamespaceDeclaration(ParseName("OpenH2.Core.Scripting.Execution"))
                            .AddUsings(
                                UsingDirective(ParseName("System")),
                                UsingDirective(ParseName("System.IO")))
                            .AddMembers(cls);
        }

        private ClassDeclarationSyntax GeneratePartialClass(INamedTypeSymbol engineInterface)
        {
            var className = "ScriptInterpreter";


            var cls = ClassDeclaration(className).WithModifiers(TokenList(Token(SyntaxKind.PartialKeyword)));


            return cls;
        }

        private INamedTypeSymbol GetType(Compilation compilation, string typeName)
        {
            return compilation.References.Select(compilation.GetAssemblyOrModuleSymbol)
                .OfType<IAssemblySymbol>()
                .Select(a => a.GetTypeByMetadataName(typeName))
                .Single(a => a != null);
        }
    }


}
