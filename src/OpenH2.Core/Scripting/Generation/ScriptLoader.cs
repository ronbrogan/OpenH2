using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Emit;
using Microsoft.CodeAnalysis.Text;
using OpenH2.Core.Tags.Scenario;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace OpenH2.Core.Scripting.Generation
{
    public class ScriptLoader
    {
        private readonly string generatedScriptOutput;
        private CSharpCompilation compilation;
        private List<EmbeddedText> embeddedSource = new List<EmbeddedText>();

        private const string AssemblyName = "OpenH2.ScriptGen";

        public ScriptLoader(string generatedScriptOutput = null)
        {
            var suppressions = new Dictionary<string, ReportDiagnostic>
            {
                { "CS1998", ReportDiagnostic.Suppress }
            };


            var compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary,
                specificDiagnosticOptions: suppressions,
                optimizationLevel: OptimizationLevel.Debug,
                platform: Platform.AnyCpu);

            var baseLibPath = Path.GetDirectoryName(typeof(object).Assembly.Location);

            var tpa = (string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES");

            this.generatedScriptOutput = generatedScriptOutput;
            this.compilation = CSharpCompilation.Create(AssemblyName, options: compilationOptions)
                .AddReferences(MetadataReference.CreateFromFile(typeof(ScriptLoader).Assembly.Location))
                .AddReferences(MetadataReference.CreateFromFile(Path.Combine(baseLibPath, "mscorlib.dll")))
                .AddReferences(MetadataReference.CreateFromFile(Path.Combine(baseLibPath, "System.dll")))
                .AddReferences(MetadataReference.CreateFromFile(Path.Combine(baseLibPath, "System.Core.dll")));

            foreach(var asmPath in tpa.Split(';'))
            {
                this.compilation = this.compilation.AddReferences(
                    MetadataReference.CreateFromFile(asmPath));
            }

            foreach(var reference in typeof(ScriptLoader).Assembly.GetReferencedAssemblies())
            {
                if(reference.CodeBase != null)
                {
                    compilation = compilation.AddReferences(MetadataReference.CreateFromFile(reference.CodeBase));
                }
            }

            var targetFrameworkUnit = SyntaxFactory.CompilationUnit()
                .AddAttributeLists(
                    SyntaxFactory.AttributeList(
                        SyntaxFactory.SingletonSeparatedList(
                            SyntaxFactory.Attribute(SyntaxFactory.QualifiedName(
                                SyntaxFactory.QualifiedName(
                                    SyntaxFactory.QualifiedName(
                                        SyntaxFactory.IdentifierName("System"),
                                        SyntaxFactory.IdentifierName("Runtime")),
                                    SyntaxFactory.IdentifierName("Versioning")),
                                SyntaxFactory.IdentifierName("TargetFrameworkAttribute")))
                                .AddArgumentListArguments(
                                    SyntaxFactory.AttributeArgument(SyntaxUtil.LiteralExpression(".NETStandard,Version=v2.1")),
                                    SyntaxFactory.AttributeArgument(SyntaxUtil.LiteralExpression(""))
                                        .WithNameEquals(SyntaxFactory.NameEquals(SyntaxFactory.IdentifierName("FrameworkDisplayName"))))))
                    .WithTarget(SyntaxFactory.AttributeTargetSpecifier(SyntaxFactory.Token(SyntaxKind.AssemblyKeyword))));

            this.compilation = this.compilation.AddSyntaxTrees(targetFrameworkUnit.SyntaxTree);

        }

        public void Load(ScenarioTag scnr)
        {
            var scenarioParts = scnr.Name.Split('\\', StringSplitOptions.RemoveEmptyEntries)
                        .Select(p => p.Trim())
                        .ToArray();

            var repo = new MemberNameRepository();

            // Reserve class field and method names

            for (var i = 0; i < scnr.ScriptVariables.Length; i++)
            {
                var variable = scnr.ScriptVariables[i];
                var returnedName = repo.RegisterName(variable.Description, variable.DataType.ToString(), i);
            }

            for (var i = 0; i < scnr.ScriptMethods.Length; i++)
            {
                var method = scnr.ScriptMethods[i];
                var returnedName = repo.RegisterName(method.Description, ScriptDataType.ScriptReference.ToString(), i);
            }

            var baseFields = typeof(ScenarioScriptBase).GetFields();

            foreach (var field in baseFields)
            {
                var found = SyntaxUtil.TryGetScriptTypeFromType(field.FieldType, out var t);

                Debug.Assert(found);

                repo.RegisterName(field.Name, t.ToString());
            }

            // Generate data properties

            var dataGen = new ScriptCSharpGenerator(scnr, repo);

            dataGen.AddProperties(scnr);

            dataGen.CreateDataInitializer(scnr);

            var originAttr = SyntaxFactory.Attribute(
                SyntaxFactory.ParseName("OriginScenario"), 
                SyntaxFactory.AttributeArgumentList(SyntaxFactory.SeparatedList(new[] {
                    SyntaxFactory.AttributeArgument(
                        SyntaxFactory.LiteralExpression(
                            SyntaxKind.StringLiteralExpression, 
                            SyntaxFactory.Literal(scnr.Name)))
                })));

            var classGen = new ScriptCSharpGenerator(scnr, repo, classAttributes: new[] { originAttr });

            foreach (var variable in scnr.ScriptVariables)
            {
                classGen.AddGlobalVariable(variable);
            }

            foreach (var script in scnr.ScriptMethods)
            {
                classGen.AddMethod(script);
            }

            this.AddCode($"{scenarioParts.Last()}.cs", classGen.Generate());
            this.AddCode($"{scenarioParts.Last()}.Data.cs", dataGen.Generate());
        }

        private void AddCode(string desiredPath, MemberDeclarationSyntax member)
        {
            var compilationUnit = SyntaxFactory.CompilationUnit().AddMembers(member);

            var sourceText = compilationUnit.ToString();
            var sourceBytes = Encoding.UTF8.GetBytes(sourceText);
            var embeddableText = SourceText.From(sourceBytes, sourceBytes.Length, Encoding.UTF8, canBeEmbedded: true);
            embeddedSource.Add(EmbeddedText.FromSource(desiredPath, embeddableText));

            var tree = CSharpSyntaxTree.Create(compilationUnit, null, desiredPath, Encoding.UTF8);
            this.compilation = this.compilation.AddSyntaxTrees(tree);

            if (string.IsNullOrWhiteSpace(this.generatedScriptOutput) == false)
            {
                File.WriteAllText(Path.Combine(this.generatedScriptOutput, desiredPath), sourceText);
            }
        }

        public Assembly CompileScripts()
        {
            var binRoot = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var dllPath = Path.Combine(binRoot, AssemblyName + ".dll");
            var pdbPath = Path.Combine(binRoot, AssemblyName + ".pdb");

            using (var pe = new FileStream(dllPath, FileMode.Create))
            using (var pdb = new FileStream(pdbPath, FileMode.Create))
            {
                var result = this.compilation.Emit(pe, pdb, options: new EmitOptions(
                    debugInformationFormat: DebugInformationFormat.PortablePdb, 
                    defaultSourceFileEncoding: Encoding.UTF8),
                    embeddedTexts: embeddedSource);

                if (result.Success == false)
                {
                    throw new Exception("Unable to emit script assembly, check dianostics\r\n" +
                        result.Diagnostics[0].Descriptor.Title);
                }
            }

            return Assembly.LoadFile(dllPath);
        }
    }
}
