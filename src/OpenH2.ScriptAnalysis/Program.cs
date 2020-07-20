using OpenH2.Core.Extensions;
using OpenH2.Core.Factories;
using OpenH2.Core.Tags.Scenario;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

            var debugTree = SExpNode.ToString(text);

            var generator = new PseudocodeGenerator();
            var pseudocode = generator.Generate(text);

            Console.WriteLine(debugTree);

            
            TextCopy.ClipboardService.SetText(pseudocode);
        }

        private static SExpNode GetScript(ScenarioTag tag, ScenarioTag.Obj440_ScriptMethod method)
        {
            var strings = (Span<byte>)tag.ScriptStrings;
            

            var root = new SExpNode()
            {
                Type = NodeType.Statement,
                Value = method.Description,
                DataType = NodeDataType.MethodOrOperator
            };

            var childIndices = new Stack<(int, SExpNode)>();
            childIndices.Push((method.ValueA, root));

            while (childIndices.Any())
            {
                var (currentIndex, parent) = childIndices.Pop();

                var node = tag.Obj568s_ScriptASTNodes[currentIndex];

                var current = new SExpNode();

                object value = node.ValueG;
                bool goodValue = false;

                if(stringLookup.TryGetValue(node.ValueG, out var gString))
                {
                    value = gString;
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
                current.DataType = (NodeDataType)node.ValueC;
                current.Type = (NodeType)node.ValueD;
                current.Value = value;
                current.Index = currentIndex;

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

        private enum NodeDataType
        {
            MethodOrOperator = 2,
            StatementStart  = 4,
            Boolean = 5,
            Short = 7,
            // 6 and 8 also appear sometimes to be strings?, h is 1 in those cases, whereas typically 0
            // but 6 has also been a numeric variable access?
            String = 9,
            AI = 19,
            AIScript = 21,
            ReferenceGet = 32,
            Entity = 50,
            Device = 54,
            EntityIdentifier = 56,
        }

        private enum NodeType
        {
            ExpressionScope = 8,
            Statement = 9,
            VariableAccess = 13
        }

        private class PseudocodeGenerator
        {
            private Stack<ScriptState> state = new Stack<ScriptState>();
            private StringBuilder builder = new StringBuilder();

            public string Generate(SExpNode root)
            {
                var nodes = new Stack<(SExpNode node, bool terminal)>();
                nodes.Push((root, true));
                nodes.Push((root, false));

                while (nodes.TryPop(out var current))
                {
                    if (current.terminal == false)
                    {
                        BeforeWrite(current.node);

                        switch (current.node.Type)
                        {
                            case NodeType.ExpressionScope:
                                switch (current.node.DataType)
                                {
                                    case NodeDataType.StatementStart:
                                        WriteStatementStart(current.node);
                                        break;
                                    case NodeDataType.Boolean:
                                        builder.Append($"Func<{current.node.DataType}>(");
                                        break;
                                    default:
                                        builder.Append($"BT<{current.node.DataType}>({current.node.Value})");
                                        break;
                                }
                                break;
                            case NodeType.Statement:
                                switch (current.node.DataType)
                                {
                                    case NodeDataType.MethodOrOperator:
                                        WriteMethodCall(current.node);
                                        break;
                                    case NodeDataType.ReferenceGet:
                                        WriteReferenceGet(current.node);
                                        break;
                                    case NodeDataType.StatementStart:
                                        builder.Append($"WARN: statement start data type inside a statement node");
                                        break;
                                    case NodeDataType.Short:
                                        WriteShortLiteral(current.node);
                                        break;
                                    case NodeDataType.Boolean:
                                        WriteBoolean(current.node);
                                        break;
                                    case NodeDataType.String:
                                        WriteStringLiteral(current.node);
                                        break;
                                    default:
                                        builder.Append($"T<{current.node.DataType}>({current.node.Value})");
                                        break;
                                }
                                break;
                            case NodeType.VariableAccess:
                                WriteVariableAccess(current.node);
                                break;
                            default:
                                builder.Append($"Unknown Syntax - {current.node.Value} S[{current.node.Type}] T[{current.node.DataType}]");
                                break;
                        }

                        

                        for (var i = current.Item1.Children.Count - 1; i >= 0; i--)
                        {
                            var c = current.Item1.Children[i];
                            nodes.Push((c, true));
                        }

                        for (var i = current.Item1.Children.Count - 1; i >= 0; i--)
                        {
                            var c = current.Item1.Children[i];
                            nodes.Push((c, false));
                        }
                    }
                    else
                    {
                        switch (current.node.Type)
                        {
                            case NodeType.ExpressionScope:
                                break;
                            case NodeType.Statement:
                            case NodeType.VariableAccess:
                                switch (current.node.DataType)
                                {
                                    case NodeDataType.MethodOrOperator:
                                        WriteMethodCallEnd(current.node);
                                        break;
                                    case NodeDataType.ReferenceGet:
                                        break;
                                    case NodeDataType.StatementStart:
                                        WriteStatementStartEnd(current.node);
                                        break;
                                    case NodeDataType.Short:
                                        break;
                                    case NodeDataType.String:
                                        break;
                                }
                                break;
                        }
                    }
                }

                return builder.ToString();
            }

            

            private void BeforeWrite(SExpNode node)
            {
                if(state.TryPeek(out var currentState) == false)
                {
                    return;
                }

                switch (currentState)
                {
                    case ScriptState.ScopeStarted:
                        break;
                    case ScriptState.MethodStarted:
                        state.Push(ScriptState.WritingMethodArguments);
                        break;
                    case ScriptState.WritingMethodArguments:
                        builder.Append(", ");
                        break;
                }
            }

            private void WriteMethodCall(SExpNode node)
            {
                builder.Append(node.Value).Append("(");
                state.Push(ScriptState.MethodStarted);
            }

            private void WriteMethodCallEnd(SExpNode node)
            {
                builder.Append(")");
                var popped = state.Pop();
                Debug.Assert(popped == ScriptState.MethodStarted || popped == ScriptState.WritingMethodArguments);
            }

            private void WriteReferenceGet(SExpNode node)
            {
                builder.Append("ref(\"").Append(node.Value).Append("\")");
            }

            private void WriteVariableAccess(SExpNode node)
            {
                builder.Append($"var<{node.DataType.ToString()}>(\"").Append(node.Value).Append("\")");
            }

            private void WriteStatementStart(SExpNode node)
            {
                // TODO: indenting?
                builder.AppendLine();
            }

            private void WriteStatementStartEnd(SExpNode node)
            {
                // TODO: decrement indent?
            }

            private void WriteStringLiteral(SExpNode node)
            {
                builder.Append("\"").Append(node.Value).Append("\"");
            }

            private void WriteShortLiteral(SExpNode node)
            {
                builder.Append(node.Original.ValueI);
            }

            private void WriteIntLiteral(SExpNode node)
            {
                builder.Append((node.Original.ValueJ << 16) | node.Original.ValueI);
            }

            private void WriteBoolean(SExpNode node)
            {
                var val = (node.Original.ValueI & 0xFF) == 0 ? "false" : "true";
                builder.Append(val);
            }

            private enum ScriptState
            {
                ScopeStarted,
                MethodStarted,
                WritingMethodArguments,

            }
        }

        private class SExpNode
        {
            public NodeType Type { get; set; }
            public NodeDataType DataType { get; set; }
            public object Value { get; set; }
            public List<SExpNode> Children { get; set; } = new List<SExpNode>();

            public ScenarioTag.Obj568_ScriptASTNode Original { get; set; }
            public ushort dEnum { get; internal set; }
            public ushort hEnum { get; internal set; }
            public int Index { get; internal set; }

            public static string ToString(SExpNode root)
            {
                var b = new StringBuilder();
                
                var nodes = new Stack<(SExpNode node, int indentLevel, bool terminal)>();
                nodes.Push((root, 0, true));
                nodes.Push((root, 0, false));

                while(nodes.TryPop(out var current))
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

                        b.Append($", {current.node.Type}<{current.node.DataType}> @:{current.node.Index} a:{orig?.ValueA},b:{orig?.ValueB},c:{orig?.ValueC},d:{orig?.ValueD},e:{orig?.ValueE},f:{orig?.ValueF},g:{orig?.ValueG},h:{orig?.ValueH},i:{orig?.ValueI},j:{orig?.ValueJ}");

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
