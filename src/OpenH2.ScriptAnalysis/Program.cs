using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using OpenH2.Core.Extensions;
using OpenH2.Core.Factories;
using OpenH2.Core.Scripting;
using OpenH2.Core.Tags.Scenario;
using OpenH2.ScriptAnalysis.GenerationState;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace OpenH2.ScriptAnalysis
{
    public class EngineMethodInfo
    {
        public string Name { get; set; }
        public ScriptDataType ReturnType { get; set; }
        public List<ScriptDataType> ArgumentTypes { get; } = new List<ScriptDataType>();
    }

    partial class Program
    {
        public static List<EngineMethodInfo> engineMethodInfos = new List<EngineMethodInfo>();

        static void Main(string[] args)
        {
            var maps = Directory.GetFiles(@"D:\H2vMaps", "*.map");

            foreach(var map in maps)
            {
                var factory = new MapFactory(Path.GetDirectoryName(map), NullMaterialFactory.Instance);
                var scene = factory.FromFile(File.OpenRead(map));

                var scnr = scene.GetLocalTagsOfType<ScenarioTag>().First();

                var scenarioParts = scnr.Name.Split('\\', StringSplitOptions.RemoveEmptyEntries)
                        .Select(p => p.Trim())
                        .ToArray();

                var outRoot = $@"D:\h2scratch\scripts\{scenarioParts.Last()}";
                Directory.CreateDirectory(outRoot);

                var csharpGen = new ScriptCSharpGenerator(scnr);

                csharpGen.OnNodeEnd += CsharpGen_OnNodeEnd;

                foreach (var variable in scnr.ScriptVariables)
                {
                    csharpGen.AddGlobalVariable(variable);
                }

                foreach (var script in scnr.ScriptMethods)
                {
                    var text = GetScriptTree(scnr, script);
                    var debugTree = ScriptTreeNode.ToString(text);
                    File.WriteAllText(Path.Combine(outRoot, script.Description + ".tree"), debugTree);

                    csharpGen.AddMethod(script);
                }

                var csharp = csharpGen.Generate();
                File.WriteAllText(outRoot + $"\\{scenarioParts.Last()}.cs", csharp);
            }            

            DedupeMethodInfos();
            var tree = GenerateScriptEngineClass();
        }

        private static void CsharpGen_OnNodeEnd(GenerationCallbackArgs args)
        {
            if(args.State is MethodCallData m)
            {
                var info = new EngineMethodInfo()
                {
                    Name = m.MethodName,
                    ReturnType = m.ReturnType
                };

                foreach(var meta in m.Metadata)
                {
                    if(meta is ScenarioTag.ScriptSyntaxNode argNode)
                    {
                        info.ArgumentTypes.Add(argNode.DataType);
                    }
                    else
                    {
                        //?
                    }
                }

                engineMethodInfos.Add(info);
            }
        }

        // TODO: Using adhoc ScriptTreeNode until we're in a good state to build a CSharpSyntaxTree directly
        private static ScriptTreeNode GetScriptTree(ScenarioTag tag, ScenarioTag.ScriptMethodDefinition method)
        {
            var strings = (Span<byte>)tag.ScriptStrings;
            

            var root = new ScriptTreeNode()
            {
                Type = NodeType.ExpressionScope,
                Value = method.Description,
                DataType = method.ReturnType,
                Original = new ScenarioTag.ScriptSyntaxNode()
                {
                    NextIndex = method.SyntaxNodeIndex,
                    NextCheckval = method.ValueB
                }
            };

            var childIndices = new Stack<(int, ScriptTreeNode)>();
            childIndices.Push((method.SyntaxNodeIndex, root));

            while (childIndices.Any())
            {
                var (currentIndex, parent) = childIndices.Pop();

                var node = tag.ScriptSyntaxNodes[currentIndex];

                var current = new ScriptTreeNode();

                object value = node.NodeData_32;

                if(node.NodeType == NodeType.Statement)
                {
                    switch (node.DataType)
                    {
                        case ScriptDataType.Boolean:
                            value = node.NodeData_B3 == 1;
                            break;
                        case ScriptDataType.Short:
                            value = node.NodeData_H16;
                            break;
                        case ScriptDataType.String:
                        case ScriptDataType.MethodOrOperator:
                        case ScriptDataType.AI:
                        case ScriptDataType.AIScript:
                        case ScriptDataType.ReferenceGet:
                        case ScriptDataType.Device:
                        case ScriptDataType.EntityIdentifier:
                            value = strings.ReadStringStarting(node.NodeString);
                            break;
                        case ScriptDataType.Entity:
                            break;
                        default:
                            // TODO: hack until everything is tracked down, populating string as value if exists
                            if (node.NodeString > 0 && node.NodeString < tag.ScriptStrings.Length
                                && tag.ScriptStrings[node.NodeString - 1] == 0)
                            {
                                value = SpanByteExtensions.ReadStringStarting(tag.ScriptStrings, node.NodeString);
                            }
                            break;
                    }
                }
                else if (node.NodeType == NodeType.VariableAccess)
                {
                    value = strings.ReadStringStarting(node.NodeString);
                }

                current.Original = node;
                current.DataType = node.DataType;
                current.Type = node.NodeType;
                current.Value = value;
                current.Index = currentIndex;

                parent.Children.Add(current);

                var nextNodeParent = current;

                // Expression scope seems to use NodeData to specify what is inside the scope
                // and the Next value is used to specify the scope's next sibling instead
                // This is how the linear-ish node structure can expand into a more traditional AST
                if(node.NodeType == NodeType.ExpressionScope)
                {
                    // Use scope's parent as next node's parent instead of the scope
                    // This makes the 'next' into a 'sibling'
                    nextNodeParent = parent;

                    Debug.Assert(tag.ScriptSyntaxNodes[node.NodeData_H16].Checkval == node.NodeData_L16, "Scope's next node checkval didn't match");
                    childIndices.Push((node.NodeData_H16, current));
                }

                // Push NextIndex using the appropriate parent node
                if (node.NextIndex != ushort.MaxValue)
                {
                    Debug.Assert(tag.ScriptSyntaxNodes[node.NextIndex].Checkval == node.NextCheckval, "Node's next checkval didn't match");
                    childIndices.Push((node.NextIndex, nextNodeParent));
                }
            }

            return root;
        }

        static string CamelCase(string s)
        {
            var x = s.Replace("_", "");
            if (x.Length == 0) return "Null";
            x = Regex.Replace(x, "([A-Z])([A-Z]+)($|[A-Z])",
                m => m.Groups[1].Value + m.Groups[2].Value.ToLower() + m.Groups[3].Value);
            return char.ToLower(x[0]) + x.Substring(1);
        }

        private static SyntaxTree GenerateScriptEngineClass()
        {
            var classMethods = new List<MemberDeclarationSyntax>();

            var camelCase = new Regex("^([A-Z])", RegexOptions.Compiled);

            foreach (var method in engineMethodInfos.OrderBy(i => i.Name))
            {
                var paramList = method.ArgumentTypes.Select((a, i) => Parameter(Identifier(CamelCase(a.ToString())))
                    .WithType(SyntaxUtil.ScriptTypeSyntax(a)));

                classMethods.Add(MethodDeclaration(SyntaxUtil.ScriptTypeSyntax(method.ReturnType), method.Name)
                    .WithModifiers(new SyntaxTokenList(Token(SyntaxKind.PublicKeyword), Token(SyntaxKind.StaticKeyword)))
                    .WithParameterList(ParameterList(SeparatedList<ParameterSyntax>(paramList)))
                    .WithBody(Block()));
            }

            var classDecl = ClassDeclaration("ScriptEngine")
                .WithModifiers(SyntaxTokenList.Create(Token(SyntaxKind.PublicKeyword)))
                .WithMembers(new SyntaxList<MemberDeclarationSyntax>(classMethods));

            var ns = NamespaceDeclaration(ParseName("OpenH2.Engine.Scripting"))
                .WithMembers(List<MemberDeclarationSyntax>().Add(classDecl))
                .AddUsings(UsingDirective(ParseName("OpenH2.Core.Scripting")));

            return CSharpSyntaxTree.Create(ns.NormalizeWhitespace());
        }

        private static void DedupeMethodInfos()
        {
            var allInfos = new Queue<EngineMethodInfo>(engineMethodInfos);
            var prunedMethodInfos = new List<EngineMethodInfo>();
            // Prune method definitions
            while (allInfos.TryDequeue(out var candidate))
            {
                var hasDuplicate = false;

                foreach (var existing in prunedMethodInfos)
                {
                    var isExactMatch = true;

                    if (existing.Name != candidate.Name) isExactMatch = false;
                    if (existing.ReturnType != candidate.ReturnType) isExactMatch = false;

                    if (existing.ArgumentTypes.Count == candidate.ArgumentTypes.Count)
                    {
                        for (var i = 0; i < existing.ArgumentTypes.Count; i++)
                        {
                            var l = existing.ArgumentTypes[i];
                            var r = candidate.ArgumentTypes[i];

                            if (l != r) isExactMatch = false;
                        }
                    }
                    else
                    {
                        isExactMatch = false;
                    }

                    if (isExactMatch)
                    {
                        hasDuplicate = true;
                    }
                }

                if(hasDuplicate == false)
                {
                    prunedMethodInfos.Add(candidate);
                }
            }

            engineMethodInfos = prunedMethodInfos;
        }
    }

    public class ScriptTreeNode
    {
        public NodeType Type { get; set; }
        public ScriptDataType DataType { get; set; }
        public object Value { get; set; }
        public List<ScriptTreeNode> Children { get; set; } = new List<ScriptTreeNode>();
        public ScenarioTag.ScriptSyntaxNode Original { get; set; }
        public int Index { get; internal set; }

        public static string ToString(ScriptTreeNode root)
        {
            var b = new StringBuilder();

            var nodes = new Stack<(ScriptTreeNode node, int indentLevel, bool terminal)>();
            nodes.Push((root, 0, true));
            nodes.Push((root, 0, false));

            while (nodes.TryPop(out var current))
            {
                if (current.terminal == false)
                {
                    var indent = new string(' ', current.indentLevel * 4);

                    var orig = current.node.Original;

                    b.AppendLine()
                        .Append(indent)
                        .Append("(");

                    if (current.node.Value.GetType() == typeof(string))
                    {
                        b.Append($"\"{current.node.Value}\"");
                    }
                    else
                    {
                        b.Append(current.node.Value);
                    }

                    b.Append($", {current.node.Type}<{current.node.DataType}> @:{current.node.Index} a:{orig?.Checkval},b:{orig?.ScriptIndex},c:{(ushort?)orig?.DataType},d:{(ushort?)orig?.NodeType},e:{orig?.NextIndex},f:{orig?.NextCheckval},g:{orig?.NodeString},h:{orig?.ValueH},i:{orig?.NodeData_H16},j:{orig?.NodeData_L16}");

                    for (var i = current.Item1.Children.Count - 1; i >= 0; i--)
                    {
                        var c = current.Item1.Children[i];
                        nodes.Push((c, current.indentLevel + 1, true));
                        nodes.Push((c, current.indentLevel + 1, false));
                    }
                }
                else
                {
                    b.Append(")");
                }
            }

            return b.ToString();
        }
    }
}
