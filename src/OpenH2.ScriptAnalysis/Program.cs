using Microsoft.CodeAnalysis;
using OpenH2.Core.Extensions;
using OpenH2.Core.Factories;
using OpenH2.Core.Scripting;
using OpenH2.Core.Scripting.Generation;
using OpenH2.Core.Tags.Scenario;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace OpenH2.ScriptAnalysis
{
    class Program
    {
        static void Main(string[] args)
        {
            var outRoot = $@"D:\h2scratch\scripts";
            Directory.CreateDirectory(outRoot);

            var loader = new ScriptLoader(outRoot);
            var maps = Directory.GetFiles(@"D:\H2vMaps", "*.map");

            foreach(var map in maps)
            {
                if (map.Contains("0") == false)
                {
                    continue;
                }

                var factory = new MapFactory(Path.GetDirectoryName(map), NullMaterialFactory.Instance);
                var scene = factory.FromFile(File.OpenRead(map));

                var scnr = scene.GetLocalTagsOfType<ScenarioTag>().First();
                loader.Load(scnr);

                var scenarioParts = scnr.Name.Split('\\', StringSplitOptions.RemoveEmptyEntries)
                        .Select(p => p.Trim())
                        .ToArray();

                var debugRoot = $@"{outRoot}\{scenarioParts.Last()}";
                Directory.CreateDirectory(debugRoot);

                foreach (var script in scnr.ScriptMethods)
                {
                    var text = GetScriptTree(scnr, script);
                    var debugTree = ScriptTreeNode.ToString(text);
                    File.WriteAllText(Path.Combine(debugRoot, script.Description + ".tree"), debugTree);
                }
            }
        }

        // TODO: Using adhoc ScriptTreeNode until we're in a good state to build a CSharpSyntaxTree directly
        private static ScriptTreeNode GetScriptTree(ScenarioTag tag, ScenarioTag.ScriptMethodDefinition method)
        {
            var strings = (Span<byte>)tag.ScriptStrings;

            var root = new ScriptTreeNode()
            {
                Type = NodeType.Scope,
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

                if(node.NodeType == NodeType.Expression)
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
                        case ScriptDataType.Sound:
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
                else if(node.NodeType == NodeType.ScriptInvocation)
                {
                    value = tag.ScriptMethods[node.ScriptIndex].Description;
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
                if(node.NodeType == NodeType.Scope || node.NodeType == NodeType.ScriptInvocation)
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
