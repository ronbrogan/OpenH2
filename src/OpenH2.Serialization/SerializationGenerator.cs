using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using OpenH2.Serialization.Layout;
using OpenH2.Serialization.Materialization;
using OpenH2.Serialization.Metadata;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace OpenH2.Serialization
{
    [Generator]
    public partial class SerializationGenerator : ISourceGenerator
    {
        private INamedTypeSymbol SerializableTypeAttribute = null;
        private HashSet<string> generatedFilenames = new HashSet<string>();

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new TypeDiscoverer());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.DebugSerializerGeneration", out var debug)
                && bool.TryParse(debug, out var bDebug) && bDebug)
            {
                Debugger.Launch();
            }

            this.SerializableTypeAttribute = context.Compilation.GetTypeSymbol<SerializableTypeAttribute>();

            if (context.SyntaxReceiver is TypeDiscoverer typeDiscoverer)
            {
                var wellKnown = new WellKnown(context.Compilation, typeDiscoverer);

                foreach (var decl in typeDiscoverer.Types)
                {
                    if (decl.SyntaxTree != null)
                    {
                        var model = context.Compilation.GetSemanticModel(decl.SyntaxTree, ignoreAccessibility: true);

                        if (IsSerializableType(model, decl, out var declType) == false)
                        {
                            continue;
                        }

                        var layoutInfo = LayoutInfo.Create(context.Compilation, declType);

                        var fullName = GetFullTypeName(decl);

                        // Duplicate declarations due to partial class. All member should be generated though, 
                        // since we're using the semantic model to get the members?
                        if(generatedFilenames.Contains(fullName))
                        {
                            continue;
                        }

                        generatedFilenames.Add(fullName);

                        var cls = GetSerializationClass(fullName);

                        var deserCtx = new DeserializerGenerateContext(model, declType, wellKnown);
                        deserCtx.Generate(ref cls, layoutInfo);

                        // TODO: Serialize generation

                        var ns = NamespaceDeclaration(ParseName(context.Compilation.AssemblyName + ".__GeneratedSerializers"))
                            .AddUsings(
                                UsingDirective(ParseName("System")),
                                UsingDirective(ParseName("System.IO")),
                                UsingDirective(ParseName(typeof(BlamSerializer).Namespace)),
                                UsingDirective(ParseName(typeof(SerializationClassAttribute).Namespace)),
                                UsingDirective(ParseName(typeof(SpanByteExtensions).Namespace)),
                                UsingDirective(ParseName(declType.ContainingNamespace.ToDisplayString())))
                            .AddMembers(cls);

                        var source = ns.NormalizeWhitespace().ToString();

                        var fileName = fullName + ".GeneratedSerializer.cs";

                        if (context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.GenerateSerializersInto", out var p))
                        {
                            try
                            {
                                if (Directory.Exists(p) == false) Directory.CreateDirectory(p);
                                File.WriteAllText(Path.Combine(p, fileName), source);
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine(e.Message);
                            }
                        }

                        context.AddSource(fileName, SourceText.From(source, Encoding.UTF8));
                    }
                }
            }
        }

        private bool IsSerializableType(SemanticModel model, TypeDeclarationSyntax decl, out INamedTypeSymbol declType)
        {
            declType = default;
            var declSymbol = model.GetDeclaredSymbol(decl);

            if (declSymbol is INamedTypeSymbol declTypeSymbol)
            {
                if (declTypeSymbol.IsAbstract)
                    return false;

                declType = declTypeSymbol;
                List<AttributeData> attrs = new List<AttributeData>();

                var topType = declType;

                while (topType != null)
                {
                    attrs.AddRange(topType.GetAttributes());
                    topType = topType.BaseType;
                }

                foreach (var attr in attrs)
                {
                    if (SymbolEqualityComparer.Default.Equals(attr.AttributeClass, this.SerializableTypeAttribute))
                    {
                        return true;
                    }

                    if (SymbolEqualityComparer.Default.Equals(attr.AttributeClass.BaseType, this.SerializableTypeAttribute))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private ClassDeclarationSyntax GetSerializationClass(string fullName)
        {
            return ClassDeclaration(fullName + "_GeneratedSerializer")
                .AddModifiers(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword))
                .AddAttributeLists(AttributeList(SeparatedList(new[] {
                    Attribute(ParseName(typeof(SerializationClassAttribute).FullName)),
                    //SyntaxFactory.Attribute(SyntaxFactory.ParseName(typeof(GeneratedCodeAttribute).FullName))
                })));
        }

        private string GetFullTypeName(TypeDeclarationSyntax decl)
        {
            if (decl.Parent is TypeDeclarationSyntax parent)
            {
                return GetFullTypeName(parent) + "_" + decl.Identifier.ValueText;
            }
            else
            {
                return decl.Identifier.ValueText;
            }
        }
    }
}
