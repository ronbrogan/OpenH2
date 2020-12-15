using OpenH2.Core.Tags.Scenario;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenH2.Core.Scripting.LowLevel
{
    public class ScriptTreeNode
    {
        public NodeType Type { get; set; }
        public ScriptDataType DataType { get; set; }
        public object Value { get; set; }
        public List<ScriptTreeNode> Children { get; set; } = new List<ScriptTreeNode>();
        public ScenarioTag.ScriptSyntaxNode Original { get; set; }
        public int Index { get; internal set; }

        public override string ToString()
        {
            return ToString(verbose: false);
        }

        public string ToString(bool verbose)
        {
            var root = this;
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
                    else if (orig.DataType == ScriptDataType.Float)
                    {
                        b.Append(BitConverter.Int32BitsToSingle((int)orig.NodeData_32)).Append("f");
                    }
                    else if (orig.NodeType == NodeType.Scope)
                    {
                        b.Append("____");
                    }
                    else
                    {
                        b.Append(current.node.Value);
                    }

                    if(verbose)
                    {
                        b.Append($", {current.node.Type}<{current.node.DataType}> @:{current.node.Index} check:{orig?.Checkval},op:{orig?.OperationId},dt:{(ushort?)orig?.DataType},nt:{(ushort?)orig?.NodeType},next:{orig?.NextIndex},nextCheck:{orig?.NextCheckval},str:{orig?.NodeString},h:{orig?.ValueH},d0:{orig?.NodeData_H16},d1:{orig?.NodeData_L16};");
                    }
                    else
                    {
                        b.Append($", {current.node.Type}<{current.node.DataType}> @:{current.node.Index} op:{orig?.OperationId},dt:{(ushort?)orig?.DataType},nt:{(ushort?)orig?.NodeType},next:{orig?.NextIndex},str:{orig?.NodeString},d0:{orig?.NodeData_H16},d1:{orig?.NodeData_L16};");
                    }

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
