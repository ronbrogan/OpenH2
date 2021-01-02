using OpenBlam.Core.Extensions;
//using OpenH2.Core.Extensions;
using OpenH2.Core.Tags.Scenario;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace OpenH2.Core.Scripting.LowLevel
{
    public static class ScriptProcessor
    {
        public static ScriptTreeNode GetScriptTree(ScenarioTag tag, ScenarioTag.ScriptMethodDefinition method, int methodIndex)
        {
            var strings = (Span<byte>)tag.ScriptStrings;

            var root = new ScriptTreeNode()
            {
                Type = NodeType.MethodDecl,
                Value = method.Description,
                DataType = method.ReturnType,
                Index = methodIndex,
                Original = new ScenarioTag.ScriptSyntaxNode()
                {
                    NextIndex = method.SyntaxNodeIndex,
                    NextCheckval = method.ValueB,
                    NodeType = NodeType.MethodDecl,
                    NodeString = (ushort)FindStringIndex(method.Description, strings)
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

                if (node.NodeType == NodeType.Expression)
                {
                    switch (node.DataType)
                    {
                        case ScriptDataType.Boolean:
                            value = node.NodeData_B3 == 1;
                            break;
                        case ScriptDataType.Short:
                            value = node.NodeData_H16;
                            break;
                        case ScriptDataType.Float:
                            value = BitConverter.Int32BitsToSingle((int)node.NodeData_32);
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
                                value = OpenBlam.Core.Extensions.SpanByteExtensions.ReadStringStarting(tag.ScriptStrings, node.NodeString);
                            }
                            break;
                    }
                }
                else if (node.NodeType == NodeType.VariableAccess)
                {
                    value = strings.ReadStringStarting(node.NodeString);
                }
                else if (node.NodeType == NodeType.ScriptInvocation)
                {
                    value = tag.ScriptMethods[node.OperationId].Description;
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
                if (node.NodeType == NodeType.BuiltinInvocation || node.NodeType == NodeType.ScriptInvocation)
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

        private static int FindStringIndex(string value, Span<byte> internedStrings)
        {
            var searchBytes = Encoding.UTF8.GetBytes(value);

            for(var i = 0; i < internedStrings.Length - searchBytes.Length; i++)
            {
                // Only scan when we're on a null character
                if (internedStrings[i] != 0) continue;

                // Only scan when it's the right length
                if (internedStrings[i + searchBytes.Length + 1] != 0) continue;

                var found = true;
                for (var j = 0; j < searchBytes.Length; j++)
                {
                    // +1 to account for the leading null char
                    if(internedStrings[i+1+j] != searchBytes[j])
                    {
                        found = false;
                        break;
                    }
                }


                if(found)
                {
                    // If we made it here, we have a match
                    return i + 1;
                }
            }

            return -1;
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
}
