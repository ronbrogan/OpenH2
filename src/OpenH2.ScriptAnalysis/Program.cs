using OpenH2.Core.Extensions;
using OpenH2.Core.Factories;
using OpenH2.Core.Scripting;
using OpenH2.Core.Tags.Scenario;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace OpenH2.ScriptAnalysis
{
    partial class Program
    {
        private static Dictionary<int, string> stringLookup;

        static void Main(string[] args)
        {
            var path = @"D:\H2vMaps\01a_tutorial.map";
            var scriptIndex = 72;//70;

            var factory = new MapFactory(Path.GetDirectoryName(path), NullMaterialFactory.Instance);
            var scene = factory.FromFile(File.OpenRead(path));

            var scnr = scene.GetLocalTagsOfType<ScenarioTag>().First();

            stringLookup = new Dictionary<int, string>();

            var lastIndex = 0;
            for(var i = 0; i < scnr.ScriptStrings.Length; i++)
            {
                if(scnr.ScriptStrings[i] == 0)
                {
                    stringLookup[lastIndex] = SpanByteExtensions.ReadStringStarting(scnr.ScriptStrings, lastIndex);
                    lastIndex = i + 1;
                }
            }

            var script = scnr.ScriptMethods[scriptIndex];

            var text = GetScriptTree(scnr, script);

            var debugTree = ScriptTreeNode.ToString(text);

            var generator = new PseudocodeGenerator();
            var pseudocode = generator.Generate(text);

            var scenarioParts = scnr.Name.Split('\\', StringSplitOptions.RemoveEmptyEntries)
                .Select(p => p.Trim())
                .ToArray();

            var ns = "OpenH2.Scripts." + string.Join('.', scenarioParts.Take(2));

            var csharpGen = new ScriptCSharpGenerator(scnr);
            csharpGen.AddMethod(script);
            var csharp = csharpGen.Generate();

            var outRoot = $@"D:\h2scratch\{script.Description}";

            File.WriteAllText(outRoot + ".tree", debugTree);
            File.WriteAllText(outRoot + ".pseudo.cs", pseudocode);
            File.WriteAllText(outRoot + ".cs", csharp);
        }

        // TODO: Using adhoc ScriptTreeNode until we're in a good state to build a CSharpSyntaxTree directly
        private static ScriptTreeNode GetScriptTree(ScenarioTag tag, ScenarioTag.ScriptMethodDefinition method)
        {
            var strings = (Span<byte>)tag.ScriptStrings;
            

            var root = new ScriptTreeNode()
            {
                Type = NodeType.Statement,
                Value = method.Description,
                DataType = NodeDataType.MethodOrOperator
            };

            var childIndices = new Stack<(int, ScriptTreeNode)>();
            childIndices.Push((method.ValueA, root));

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
                        case NodeDataType.Boolean:
                            value = node.NodeData_B0 == 1;
                            break;
                        case NodeDataType.Short:
                            value = node.NodeData_H16;
                            break;
                        case NodeDataType.String:
                        case NodeDataType.MethodOrOperator:
                        case NodeDataType.AI:
                        case NodeDataType.AIScript:
                        case NodeDataType.ReferenceGet:
                        case NodeDataType.Device:
                        case NodeDataType.EntityIdentifier:
                            value = strings.ReadStringStarting(node.NodeString);
                            break;
                        case NodeDataType.Entity:
                            break;
                        default:
                            // TODO: hack until everything is tracked down, populating string as value if exists
                            if(stringLookup.TryGetValue(node.NodeString, out var defaultString))
                            {
                                value = defaultString;
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

            int mask(int value) => value & 0x3FFF;
        }
    }

    public class ScriptTreeNode
    {
        public NodeType Type { get; set; }
        public NodeDataType DataType { get; set; }
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

                    b.Append($", {current.node.Type}<{current.node.DataType}> @:{current.node.Index} a:{orig?.Checkval},b:{orig?.ValueB},c:{(ushort?)orig?.DataType},d:{(ushort?)orig?.NodeType},e:{orig?.NextIndex},f:{orig?.NextCheckval},g:{orig?.NodeString},h:{orig?.ValueH},i:{orig?.NodeData_H16},j:{orig?.NodeData_L16}");

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
