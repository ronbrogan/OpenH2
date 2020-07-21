using OpenH2.Core.Scripting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace OpenH2.ScriptAnalysis
{
    partial class Program
    {
        private class PseudocodeGenerator
        {
            private Stack<ScriptState> state = new Stack<ScriptState>();
            private StringBuilder builder = new StringBuilder();

            public string Generate(ScriptTreeNode root)
            {
                var nodes = new Stack<(ScriptTreeNode node, bool terminal)>();
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
                                    default:
                                        WriteScopeStart(current.node);
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
                                    case NodeDataType.Int:
                                        WriteIntLiteral(current.node);
                                        break;
                                    case NodeDataType.Float:
                                        WriteFloatLiteral(current.node);
                                        break;
                                    case NodeDataType.Boolean:
                                        WriteBoolean(current.node);
                                        break;
                                    case NodeDataType.String:
                                        WriteStringLiteral(current.node);
                                        break;

                                    // Handle other literals
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
                            nodes.Push((c, false));
                        }
                    }
                    else
                    {
                        switch (current.node.Type)
                        {
                            case NodeType.ExpressionScope:
                                switch(current.node.DataType)
                                {
                                    case NodeDataType.StatementStart:
                                        break;
                                    default:
                                        WriteScopeEnd(current.node);
                                        break;
                                }
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

            

            private void BeforeWrite(ScriptTreeNode node)
            {
                if(state.TryPeek(out var currentState) == false)
                {
                    return;
                }

                switch (currentState)
                {
                    case ScriptState.ScopeStarted:
                        state.Push(ScriptState.WritingScopeStatements);
                        break;
                    case ScriptState.WritingScopeStatements:
                        builder.Append(" ?? ");
                        break;
                    case ScriptState.MethodStarted:
                        state.Push(ScriptState.WritingMethodArguments);
                        break;
                    case ScriptState.WritingMethodArguments:
                        builder.Append(", ");
                        break;
                }
            }

            private void WriteMethodCall(ScriptTreeNode node)
            {
                builder.Append(node.Value).Append("(");
                state.Push(ScriptState.MethodStarted);
            }

            private void WriteMethodCallEnd(ScriptTreeNode node)
            {
                builder.Append(")");
                var popped = state.Pop();
                Debug.Assert(popped == ScriptState.MethodStarted || popped == ScriptState.WritingMethodArguments);

                if(popped == ScriptState.WritingMethodArguments)
                {
                    popped = state.Pop();
                    Debug.Assert(popped == ScriptState.MethodStarted);
                }
            }

            private void WriteScopeStart(ScriptTreeNode node)
            {
                builder.Append($"Func<{node.DataType}>(");
                state.Push(ScriptState.ScopeStarted);
            }

            private void WriteScopeEnd(ScriptTreeNode node)
            {
                builder.Append(")");
                var popped = state.Pop();
                Debug.Assert(popped == ScriptState.ScopeStarted || popped == ScriptState.WritingScopeStatements);

                if (popped == ScriptState.WritingScopeStatements)
                {
                    popped = state.Pop();
                    Debug.Assert(popped == ScriptState.ScopeStarted);
                }
            }

            private void WriteReferenceGet(ScriptTreeNode node)
            {
                builder.Append("ref(\"").Append(node.Value).Append("\")");
            }

            private void WriteVariableAccess(ScriptTreeNode node)
            {
                builder.Append($"var<{node.DataType.ToString()}>(\"").Append(node.Value).Append("\")");
            }

            private void WriteStatementStart(ScriptTreeNode node)
            {
                // TODO: indenting?
                builder.AppendLine();
            }

            private void WriteStatementStartEnd(ScriptTreeNode node)
            {
                // TODO: decrement indent?
            }

            private void WriteStringLiteral(ScriptTreeNode node)
            {
                builder.Append("\"").Append(node.Value).Append("\"");
            }

            private void WriteShortLiteral(ScriptTreeNode node)
            {
                builder.Append(node.Original.NodeData_H16);
            }

            private void WriteIntLiteral(ScriptTreeNode node)
            {
                builder.Append(node.Original.NodeData_32);
            }

            private void WriteFloatLiteral(ScriptTreeNode node)
            {
                builder.Append(BitConverter.Int32BitsToSingle((int)node.Original.NodeData_32));
            }

            private void WriteBoolean(ScriptTreeNode node)
            {
                var val = (node.Original.NodeData_H16 & 0xFF) == 0 ? "false" : "true";
                builder.Append(val);
            }

            private enum ScriptState
            {
                ScopeStarted,
                MethodStarted,
                WritingMethodArguments,
                WritingScopeStatements,
            }
        }
    }
}
