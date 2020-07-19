using OpenH2.Core.Extensions;
using OpenH2.Core.Factories;
using OpenH2.Core.Tags.Scenario;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OpenH2.ScriptAnalysis
{
    class Program
    {
        private static Dictionary<int, string> stringLookup;

        static void Main(string[] args)
        {
            var path = @"D:\H2vMaps\01a_tutorial.map";
            var scriptIndex = 72;

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

            var text = GetScript(scnr, script);

            Console.WriteLine(SExpNode.ToString(text));
            TextCopy.ClipboardService.SetText(SExpNode.ToString(text));
        }

        private static SExpNode GetScript(ScenarioTag tag, ScenarioTag.Obj440_ScriptMethod method)
        {
            var strings = (Span<byte>)tag.ScriptStrings;
            

            var root = new SExpNode();

            var childIndices = new Stack<(int, SExpNode)>();
            childIndices.Push((method.ValueA, root));

            while (childIndices.Any())
            {
                var (currentIndex, parent) = childIndices.Pop();

                var node = tag.Obj568s_ScriptASTNodes[currentIndex];

                var current = new SExpNode();

                string value = node.ValueG.ToString();
                bool goodValue = false;

                if(stringLookup.TryGetValue(node.ValueG, out var gString))
                {
                    value = $"\"{gString}\"";
                    goodValue = true;
                }
                else if (stringLookup.TryGetValue(mask(node.ValueG), out var mgString))
                {
                    value = "Masked: " + mgString;
                    goodValue = true;
                }

                current.Original = node;
                current.dEnum = node.ValueD;
                current.hEnum = node.ValueH;
                current.Type = (NodeType)node.ValueC;

                value += $", [{current.Type.ToString()}] a:{node.ValueA},b:{node.ValueB},c:{node.ValueC},d:{node.ValueD},e:{node.ValueE},f:{node.ValueF},g:{node.ValueG},h:{node.ValueH},i:{node.ValueI},j:{node.ValueJ}";
                current.Value = value;

                parent.Children.Add(current);

                if (node.ValueE > 0 && node.ValueE < tag.Obj568s_ScriptASTNodes.Length
                    && node.ValueF != 0 && tag.Obj568s_ScriptASTNodes[node.ValueE].ValueA == node.ValueF)
                {
                    childIndices.Push((node.ValueE, current));
                }

                if (node.ValueI > 0 && node.ValueI < tag.Obj568s_ScriptASTNodes.Length
                    && node.ValueJ != 0 && tag.Obj568s_ScriptASTNodes[node.ValueI].ValueA == node.ValueJ)
                {
                    childIndices.Push((node.ValueI, current));
                }

                if (goodValue == false && node.ValueH == 2 && node.ValueG > 0 && node.ValueG < tag.Obj568s_ScriptASTNodes.Length)
                {
                    //childIndices.Push((node.ValueG, current));
                }
            }

            return root;

            int mask(int value) => value & 0x3FFF;
        }

        private enum NodeType
        {
            MethodOrOperator = 2,
            StatementStart  = 4,
            NumericLiteral = 7,
            StringLIteral = 9,
        }

        private class SExpNode
        {
            public NodeType Type { get; set; }
            public object Value { get; set; }
            public List<SExpNode> Children { get; set; } = new List<SExpNode>();

            public object Original { get; set; }
            public ushort dEnum { get; internal set; }
            public ushort hEnum { get; internal set; }

            public static string ToString(SExpNode root)
            {
                var b = new StringBuilder();
                
                var nodes = new Stack<(SExpNode node, int indentLevel, bool terminal)>();
                nodes.Push((root, 0, true));
                nodes.Push((root, 0, false));

                while(nodes.TryPop(out var current))
                {
                    if(current.terminal == false)
                    {
                        var indent = new string(' ', current.indentLevel * 4);
                        
                        b.AppendLine()
                            .Append(indent)
                            .Append("(")
                            .Append(current.Item1.Value?.ToString());

                        for (var i = current.Item1.Children.Count - 1; i >= 0; i--)
                        {
                            var c = current.Item1.Children[i];
                            nodes.Push((c, current.indentLevel + 1, true));
                        }

                        for (var i = current.Item1.Children.Count - 1; i >= 0; i--)
                        {
                            var c = current.Item1.Children[i];
                            nodes.Push((c, current.indentLevel + 1, false));
                        }
                    }
                    else
                    {
                        //var indent = new string(' ', Math.Max(0, (current.indentLevel) * 4);
                        b.Append(")");
                    }
                }

                return b.ToString();
            }
        }
    }
}
