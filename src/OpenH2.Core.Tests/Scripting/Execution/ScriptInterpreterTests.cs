using Moq;
using OpenH2.Core.Scripting;
using OpenH2.Core.Scripting.Execution;
using OpenH2.Core.Tags.Scenario;
using System;
using Xunit;

namespace OpenH2.Core.Tests.Scripting.Execution
{
    public class ScriptInterpreterTests
    {
        [Fact]
        public void Interpret_ExpressionReturnsResult()
        {
            var scen = new ScenarioTag(0)
            {
                ScriptSyntaxNodes = new ScenarioTag.ScriptSyntaxNode[]
                {
                    Node(NodeType.Expression, ScriptDataType.Float, op: (ushort)ScriptDataType.Float, data: From(2f)),
                }
            };

            var interpreter = new ScriptInterpreter(scen, Mock.Of<IScriptEngine>());

            var result = interpreter.Interpret(scen.ScriptSyntaxNodes[0], out var n);

            Assert.Equal(ScriptDataType.Float, result.DataType);
            Assert.Equal(2f, result.Float);
            Assert.Equal(ushort.MaxValue, n);
        }

        [Fact]
        public void Interpret_ScopeReturnsResult()
        {
            var scen = new ScenarioTag(0)
            {
                ScriptSyntaxNodes = new ScenarioTag.ScriptSyntaxNode[]
                {
                    ScopeNode(ScriptDataType.Float, op: 6, child: 1),
                    Node(NodeType.Expression, ScriptDataType.Float, op: 6, data: From(2f)),
                }
            };

            var interpreter = new ScriptInterpreter(scen, Mock.Of<IScriptEngine>());

            var result = interpreter.Interpret(scen.ScriptSyntaxNodes[0], out var n);

            Assert.Equal(ScriptDataType.Float, result.DataType);
            Assert.Equal(2f, result.Float);
            Assert.Equal(ushort.MaxValue, n);
        }

        [Fact]
        public void Interpret_ScopeCast()
        {
            var scen = new ScenarioTag(0)
            {
                ScriptSyntaxNodes = new ScenarioTag.ScriptSyntaxNode[]
                {
                    ScopeNode(ScriptDataType.Short, op: 7, child: 1),
                        Node(NodeType.Expression, ScriptDataType.MethodOrOperator, op: ScriptOps.Add, next: 2),
                            Node(NodeType.Expression, ScriptDataType.Float, op: 6, data: From(2f), next: 3),
                                Node(NodeType.Expression, ScriptDataType.Float, op: 6, data: From(2f), next: 4),
                                    Node(NodeType.Expression, ScriptDataType.Float, op: 6, data: From(2f)),
                }
            };

            var interpreter = new ScriptInterpreter(scen, Mock.Of<IScriptEngine>());

            var result = interpreter.Interpret(scen.ScriptSyntaxNodes[0], out var n);

            Assert.Equal(ScriptDataType.Short, result.DataType);
            Assert.Equal(6, result.Short);
            Assert.Equal(ushort.MaxValue, n);
        }


        [Fact]
        public void Interpret_Add()
        {
            var scen = new ScenarioTag(0)
            {
                ScriptSyntaxNodes = new ScenarioTag.ScriptSyntaxNode[]
                {
                    ScopeNode(ScriptDataType.Float, op: 6, child: 1),
                        Node(NodeType.Expression, ScriptDataType.MethodOrOperator, op: ScriptOps.Add, next: 2),
                            Node(NodeType.Expression, ScriptDataType.Float, op: 6, data: From(2f), next: 3),
                                Node(NodeType.Expression, ScriptDataType.Float, op: 6, data: From(2f), next: 4),
                                    Node(NodeType.Expression, ScriptDataType.Float, op: 6, data: From(2f)),
                }
            };

            var interpreter = new ScriptInterpreter(scen, Mock.Of<IScriptEngine>());

            var result = interpreter.Interpret(scen.ScriptSyntaxNodes[0], out var n);

            Assert.Equal(ScriptDataType.Float, result.DataType);
            Assert.Equal(6f, result.Float);
            Assert.Equal(ushort.MaxValue, n);
        }

        [Fact]
        public void Interpret_Subtract()
        {
            var scen = new ScenarioTag(0)
            {
                ScriptSyntaxNodes = new ScenarioTag.ScriptSyntaxNode[]
                {
                    ScopeNode(ScriptDataType.Float, op: 6, child: 1),
                        Node(NodeType.Expression, ScriptDataType.MethodOrOperator, op: ScriptOps.Subtract, next: 2),
                            Node(NodeType.Expression, ScriptDataType.Float, op: 6, data: From(2f), next: 3),
                                Node(NodeType.Expression, ScriptDataType.Float, op: 6, data: From(2f))
                }
            };

            var interpreter = new ScriptInterpreter(scen, Mock.Of<IScriptEngine>());

            var result = interpreter.Interpret(scen.ScriptSyntaxNodes[0], out var n);

            Assert.Equal(ScriptDataType.Float, result.DataType);
            Assert.Equal(0f, result.Float);
            Assert.Equal(ushort.MaxValue, n);
        }

        [Fact]
        public void Interpret_Muliply()
        {
            var scen = new ScenarioTag(0)
            {
                ScriptSyntaxNodes = new ScenarioTag.ScriptSyntaxNode[]
                {
                    ScopeNode(ScriptDataType.Float, op: 6, child: 1),
                        Node(NodeType.Expression, ScriptDataType.MethodOrOperator, op: ScriptOps.Multiply, next: 2),
                            Node(NodeType.Expression, ScriptDataType.Float, op: 6, data: From(2f), next: 3),
                                Node(NodeType.Expression, ScriptDataType.Float, op: 6, data: From(2f), next: 4),
                                    Node(NodeType.Expression, ScriptDataType.Float, op: 6, data: From(2f)),
                }
            };

            var interpreter = new ScriptInterpreter(scen, Mock.Of<IScriptEngine>());

            var result = interpreter.Interpret(scen.ScriptSyntaxNodes[0], out var n);

            Assert.Equal(ScriptDataType.Float, result.DataType);
            Assert.Equal(8f, result.Float);
            Assert.Equal(ushort.MaxValue, n);
        }

        [Fact]
        public void Interpret_Divide()
        {
            var scen = new ScenarioTag(0)
            {
                ScriptSyntaxNodes = new ScenarioTag.ScriptSyntaxNode[]
                {
                    ScopeNode(ScriptDataType.Float, op: 6, child: 1),
                        Node(NodeType.Expression, ScriptDataType.MethodOrOperator, op: ScriptOps.Divide, next: 2),
                            Node(NodeType.Expression, ScriptDataType.Float, op: 6, data: From(2f), next: 3),
                                Node(NodeType.Expression, ScriptDataType.Float, op: 6, data: From(2f))
                }
            };

            var interpreter = new ScriptInterpreter(scen, Mock.Of<IScriptEngine>());

            var result = interpreter.Interpret(scen.ScriptSyntaxNodes[0], out var n);

            Assert.Equal(ScriptDataType.Float, result.DataType);
            Assert.Equal(1f, result.Float);
            Assert.Equal(ushort.MaxValue, n);
        }

        [Fact]
        public void Interpret_MinA()
        {
            var scen = new ScenarioTag(0)
            {
                ScriptSyntaxNodes = new ScenarioTag.ScriptSyntaxNode[]
                {
                    ScopeNode(ScriptDataType.Float, op: 6, child: 1),
                        Node(NodeType.Expression, ScriptDataType.MethodOrOperator, op: ScriptOps.Min, next: 2),
                            Node(NodeType.Expression, ScriptDataType.Float, op: 6, data: From(1f), next: 3),
                                Node(NodeType.Expression, ScriptDataType.Float, op: 6, data: From(2f))
                }
            };

            var interpreter = new ScriptInterpreter(scen, Mock.Of<IScriptEngine>());

            var result = interpreter.Interpret(scen.ScriptSyntaxNodes[0], out var n);

            Assert.Equal(ScriptDataType.Float, result.DataType);
            Assert.Equal(1f, result.Float);
            Assert.Equal(ushort.MaxValue, n);
        }

        [Fact]
        public void Interpret_MinB()
        {
            var scen = new ScenarioTag(0)
            {
                ScriptSyntaxNodes = new ScenarioTag.ScriptSyntaxNode[]
                {
                    ScopeNode(ScriptDataType.Float, op: 6, child: 1),
                        Node(NodeType.Expression, ScriptDataType.MethodOrOperator, op: ScriptOps.Min, next: 2),
                            Node(NodeType.Expression, ScriptDataType.Float, op: 6, data: From(2f), next: 3),
                                Node(NodeType.Expression, ScriptDataType.Float, op: 6, data: From(1f))
                }
            };

            var interpreter = new ScriptInterpreter(scen, Mock.Of<IScriptEngine>());

            var result = interpreter.Interpret(scen.ScriptSyntaxNodes[0], out var n);

            Assert.Equal(ScriptDataType.Float, result.DataType);
            Assert.Equal(1f, result.Float);
            Assert.Equal(ushort.MaxValue, n);
        }

        [Fact]
        public void Interpret_MaxA()
        {
            var scen = new ScenarioTag(0)
            {
                ScriptSyntaxNodes = new ScenarioTag.ScriptSyntaxNode[]
                {
                    ScopeNode(ScriptDataType.Float, op: 6, child: 1),
                        Node(NodeType.Expression, ScriptDataType.MethodOrOperator, op: ScriptOps.Max, next: 2),
                            Node(NodeType.Expression, ScriptDataType.Float, op: 6, data: From(1f), next: 3),
                                Node(NodeType.Expression, ScriptDataType.Float, op: 6, data: From(2f))
                }
            };

            var interpreter = new ScriptInterpreter(scen, Mock.Of<IScriptEngine>());

            var result = interpreter.Interpret(scen.ScriptSyntaxNodes[0], out var n);

            Assert.Equal(ScriptDataType.Float, result.DataType);
            Assert.Equal(2f, result.Float);
            Assert.Equal(ushort.MaxValue, n);
        }

        [Fact]
        public void Interpret_MaxB()
        {
            var scen = new ScenarioTag(0)
            {
                ScriptSyntaxNodes = new ScenarioTag.ScriptSyntaxNode[]
                {
                    ScopeNode(ScriptDataType.Float, op: 6, child: 1),
                        Node(NodeType.Expression, ScriptDataType.MethodOrOperator, op: ScriptOps.Max, next: 2),
                            Node(NodeType.Expression, ScriptDataType.Float, op: 6, data: From(2f), next: 3),
                                Node(NodeType.Expression, ScriptDataType.Float, op: 6, data: From(1f))
                }
            };

            var interpreter = new ScriptInterpreter(scen, Mock.Of<IScriptEngine>());

            var result = interpreter.Interpret(scen.ScriptSyntaxNodes[0], out var n);

            Assert.Equal(ScriptDataType.Float, result.DataType);
            Assert.Equal(2f, result.Float);
            Assert.Equal(ushort.MaxValue, n);
        }

        [Fact]
        public void Interpret_EqualsFloat()
        {
            var scen = new ScenarioTag(0)
            {
                ScriptSyntaxNodes = new ScenarioTag.ScriptSyntaxNode[]
                {
                    ScopeNode(ScriptDataType.Boolean, op: 5, child: 1),
                        Node(NodeType.Expression, ScriptDataType.MethodOrOperator, op: ScriptOps.Equals, next: 2),
                            Node(NodeType.Expression, ScriptDataType.Float, op: 6, data: From(2f), next: 3),
                                Node(NodeType.Expression, ScriptDataType.Float, op: 6, data: From(2f))
                }
            };

            var interpreter = new ScriptInterpreter(scen, Mock.Of<IScriptEngine>());

            var result = interpreter.Interpret(scen.ScriptSyntaxNodes[0], out var n);

            Assert.Equal(ScriptDataType.Boolean, result.DataType);
            Assert.True(result.Boolean);
            Assert.Equal(ushort.MaxValue, n);
        }

        [Fact]
        public void Interpret_NotEqualsFloat()
        {
            var scen = new ScenarioTag(0)
            {
                ScriptSyntaxNodes = new ScenarioTag.ScriptSyntaxNode[]
                {
                    ScopeNode(ScriptDataType.Boolean, op: 5, child: 1),
                        Node(NodeType.Expression, ScriptDataType.MethodOrOperator, op: ScriptOps.Equals, next: 2),
                            Node(NodeType.Expression, ScriptDataType.Float, op: 6, data: From(2f), next: 3),
                                Node(NodeType.Expression, ScriptDataType.Float, op: 6, data: From(1f))
                }
            };

            var interpreter = new ScriptInterpreter(scen, Mock.Of<IScriptEngine>());

            var result = interpreter.Interpret(scen.ScriptSyntaxNodes[0], out var n);

            Assert.Equal(ScriptDataType.Boolean, result.DataType);
            Assert.False(result.Boolean);
            Assert.Equal(ushort.MaxValue, n);
        }

        [Fact]
        public void Interpret_EqualsInt()
        {
            var scen = new ScenarioTag(0)
            {
                ScriptSyntaxNodes = new ScenarioTag.ScriptSyntaxNode[]
                {
                    ScopeNode(ScriptDataType.Boolean, op: 5, child: 1),
                        Node(NodeType.Expression, ScriptDataType.MethodOrOperator, op: ScriptOps.Equals, next: 2),
                            Node(NodeType.Expression, ScriptDataType.Int, op: 8, data: 2, next: 3),
                                Node(NodeType.Expression, ScriptDataType.Int, op: 8, data: 2)
                }
            };

            var interpreter = new ScriptInterpreter(scen, Mock.Of<IScriptEngine>());

            var result = interpreter.Interpret(scen.ScriptSyntaxNodes[0], out var n);

            Assert.Equal(ScriptDataType.Boolean, result.DataType);
            Assert.True(result.Boolean);
            Assert.Equal(ushort.MaxValue, n);
        }

        [Fact]
        public void Interpret_NotEqualsInt()
        {
            var scen = new ScenarioTag(0)
            {
                ScriptSyntaxNodes = new ScenarioTag.ScriptSyntaxNode[]
                {
                    ScopeNode(ScriptDataType.Boolean, op: 5, child: 1),
                        Node(NodeType.Expression, ScriptDataType.MethodOrOperator, op: ScriptOps.Equals, next: 2),
                            Node(NodeType.Expression, ScriptDataType.Int, op: 8, data: 2, next: 3),
                                Node(NodeType.Expression, ScriptDataType.Int, op: 8, data: 1)
                }
            };

            var interpreter = new ScriptInterpreter(scen, Mock.Of<IScriptEngine>());

            var result = interpreter.Interpret(scen.ScriptSyntaxNodes[0], out var n);

            Assert.Equal(ScriptDataType.Boolean, result.DataType);
            Assert.False(result.Boolean);
            Assert.Equal(ushort.MaxValue, n);
        }

        [Fact]
        public void Interpret_EqualsShort()
        {
            var scen = new ScenarioTag(0)
            {
                ScriptSyntaxNodes = new ScenarioTag.ScriptSyntaxNode[]
                {
                    ScopeNode(ScriptDataType.Boolean, op: 5, child: 1),
                        Node(NodeType.Expression, ScriptDataType.MethodOrOperator, op: ScriptOps.Equals, next: 2),
                            Node(NodeType.Expression, ScriptDataType.Float, op: 6, data: 2, next: 3),
                                Node(NodeType.Expression, ScriptDataType.Float, op: 6, data: 2)
                }
            };

            var interpreter = new ScriptInterpreter(scen, Mock.Of<IScriptEngine>());

            var result = interpreter.Interpret(scen.ScriptSyntaxNodes[0], out var n);

            Assert.Equal(ScriptDataType.Boolean, result.DataType);
            Assert.True(result.Boolean);
            Assert.Equal(ushort.MaxValue, n);
        }

        [Fact]
        public void Interpret_NotEqualsShort()
        {
            var scen = new ScenarioTag(0)
            {
                ScriptSyntaxNodes = new ScenarioTag.ScriptSyntaxNode[]
                {
                    ScopeNode(ScriptDataType.Boolean, op: 5, child: 1),
                        Node(NodeType.Expression, ScriptDataType.MethodOrOperator, op: ScriptOps.Equals, next: 2),
                            Node(NodeType.Expression, ScriptDataType.Short, op: 7, data: 2, next: 3),
                                Node(NodeType.Expression, ScriptDataType.Short, op: 7, data: 1)
                }
            };

            var interpreter = new ScriptInterpreter(scen, Mock.Of<IScriptEngine>());

            var result = interpreter.Interpret(scen.ScriptSyntaxNodes[0], out var n);

            Assert.Equal(ScriptDataType.Boolean, result.DataType);
            Assert.False(result.Boolean);
            Assert.Equal(ushort.MaxValue, n);
        }

        [Fact]
        public void Interpret_LessThanShort()
        {
            var scen = new ScenarioTag(0)
            {
                ScriptSyntaxNodes = new ScenarioTag.ScriptSyntaxNode[]
                {
                    ScopeNode(ScriptDataType.Boolean, op: 5, child: 1),
                        Node(NodeType.Expression, ScriptDataType.MethodOrOperator, op: ScriptOps.LessThan, next: 2),
                            Node(NodeType.Expression, ScriptDataType.Short, op: 7, data: 1, next: 3),
                                Node(NodeType.Expression, ScriptDataType.Short, op: 7, data: 2)
                }
            };

            var interpreter = new ScriptInterpreter(scen, Mock.Of<IScriptEngine>());

            var result = interpreter.Interpret(scen.ScriptSyntaxNodes[0], out var n);

            Assert.Equal(ScriptDataType.Boolean, result.DataType);
            Assert.True(result.Boolean);
            Assert.Equal(ushort.MaxValue, n);
        }

        [Fact]
        public void Interpret_NotLessThanShort()
        {
            var scen = new ScenarioTag(0)
            {
                ScriptSyntaxNodes = new ScenarioTag.ScriptSyntaxNode[]
                {
                    ScopeNode(ScriptDataType.Boolean, op: 5, child: 1),
                        Node(NodeType.Expression, ScriptDataType.MethodOrOperator, op: ScriptOps.LessThan, next: 2),
                            Node(NodeType.Expression, ScriptDataType.Short, op: 7, data: 2, next: 3),
                                Node(NodeType.Expression, ScriptDataType.Short, op: 7, data: 1)
                }
            };

            var interpreter = new ScriptInterpreter(scen, Mock.Of<IScriptEngine>());

            var result = interpreter.Interpret(scen.ScriptSyntaxNodes[0], out var n);

            Assert.Equal(ScriptDataType.Boolean, result.DataType);
            Assert.False(result.Boolean);
            Assert.Equal(ushort.MaxValue, n);
        }

        [Fact]
        public void Interpret_LessThanOrEqualShort()
        {
            var scen = new ScenarioTag(0)
            {
                ScriptSyntaxNodes = new ScenarioTag.ScriptSyntaxNode[]
                {
                    ScopeNode(ScriptDataType.Boolean, op: 5, child: 1),
                        Node(NodeType.Expression, ScriptDataType.MethodOrOperator, op: ScriptOps.LessThanOrEqual, next: 2),
                            Node(NodeType.Expression, ScriptDataType.Short, op: 7, data: 2, next: 3),
                                Node(NodeType.Expression, ScriptDataType.Short, op: 7, data: 2)
                }
            };

            var interpreter = new ScriptInterpreter(scen, Mock.Of<IScriptEngine>());

            var result = interpreter.Interpret(scen.ScriptSyntaxNodes[0], out var n);

            Assert.Equal(ScriptDataType.Boolean, result.DataType);
            Assert.True(result.Boolean);
            Assert.Equal(ushort.MaxValue, n);
        }

        [Fact]
        public void Interpret_NotLessThanOrEqualShort()
        {
            var scen = new ScenarioTag(0)
            {
                ScriptSyntaxNodes = new ScenarioTag.ScriptSyntaxNode[]
                {
                    ScopeNode(ScriptDataType.Boolean, op: 5, child: 1),
                        Node(NodeType.Expression, ScriptDataType.MethodOrOperator, op: ScriptOps.LessThanOrEqual, next: 2),
                            Node(NodeType.Expression, ScriptDataType.Short, op: 7, data: 2, next: 3),
                                Node(NodeType.Expression, ScriptDataType.Short, op: 7, data: 1)
                }
            };

            var interpreter = new ScriptInterpreter(scen, Mock.Of<IScriptEngine>());

            var result = interpreter.Interpret(scen.ScriptSyntaxNodes[0], out var n);

            Assert.Equal(ScriptDataType.Boolean, result.DataType);
            Assert.False(result.Boolean);
            Assert.Equal(ushort.MaxValue, n);
        }

        [Fact]
        public void Interpret_GreaterThanShort()
        {
            var scen = new ScenarioTag(0)
            {
                ScriptSyntaxNodes = new ScenarioTag.ScriptSyntaxNode[]
                {
                    ScopeNode(ScriptDataType.Boolean, op: 5, child: 1),
                        Node(NodeType.Expression, ScriptDataType.MethodOrOperator, op: ScriptOps.GreaterThan, next: 2),
                            Node(NodeType.Expression, ScriptDataType.Short, op: 7, data: 2, next: 3),
                                Node(NodeType.Expression, ScriptDataType.Short, op: 7, data: 1)
                }
            };

            var interpreter = new ScriptInterpreter(scen, Mock.Of<IScriptEngine>());

            var result = interpreter.Interpret(scen.ScriptSyntaxNodes[0], out var n);

            Assert.Equal(ScriptDataType.Boolean, result.DataType);
            Assert.True(result.Boolean);
            Assert.Equal(ushort.MaxValue, n);
        }

        [Fact]
        public void Interpret_NotGreaterThanShort()
        {
            var scen = new ScenarioTag(0)
            {
                ScriptSyntaxNodes = new ScenarioTag.ScriptSyntaxNode[]
                {
                    ScopeNode(ScriptDataType.Boolean, op: 5, child: 1),
                        Node(NodeType.Expression, ScriptDataType.MethodOrOperator, op: ScriptOps.GreaterThan, next: 2),
                            Node(NodeType.Expression, ScriptDataType.Short, op: 7, data: 1, next: 3),
                                Node(NodeType.Expression, ScriptDataType.Short, op: 7, data: 2)
                }
            };

            var interpreter = new ScriptInterpreter(scen, Mock.Of<IScriptEngine>());

            var result = interpreter.Interpret(scen.ScriptSyntaxNodes[0], out var n);

            Assert.Equal(ScriptDataType.Boolean, result.DataType);
            Assert.False(result.Boolean);
            Assert.Equal(ushort.MaxValue, n);
        }

        [Fact]
        public void Interpret_GreaterThanOrEqualShort()
        {
            var scen = new ScenarioTag(0)
            {
                ScriptSyntaxNodes = new ScenarioTag.ScriptSyntaxNode[]
                {
                    ScopeNode(ScriptDataType.Boolean, op: 5, child: 1),
                        Node(NodeType.Expression, ScriptDataType.MethodOrOperator, op: ScriptOps.GreaterThanOrEqual, next: 2),
                            Node(NodeType.Expression, ScriptDataType.Short, op: 7, data: 2, next: 3),
                                Node(NodeType.Expression, ScriptDataType.Short, op: 7, data: 2)
                }
            };

            var interpreter = new ScriptInterpreter(scen, Mock.Of<IScriptEngine>());

            var result = interpreter.Interpret(scen.ScriptSyntaxNodes[0], out var n);

            Assert.Equal(ScriptDataType.Boolean, result.DataType);
            Assert.True(result.Boolean);
            Assert.Equal(ushort.MaxValue, n);
        }

        [Fact]
        public void Interpret_NotGreaterThanOrEqualShort()
        {
            var scen = new ScenarioTag(0)
            {
                ScriptSyntaxNodes = new ScenarioTag.ScriptSyntaxNode[]
                {
                    ScopeNode(ScriptDataType.Boolean, op: 5, child: 1),
                        Node(NodeType.Expression, ScriptDataType.MethodOrOperator, op: ScriptOps.GreaterThanOrEqual, next: 2),
                            Node(NodeType.Expression, ScriptDataType.Short, op: 7, data: 1, next: 3),
                                Node(NodeType.Expression, ScriptDataType.Short, op: 7, data: 2)
                }
            };

            var interpreter = new ScriptInterpreter(scen, Mock.Of<IScriptEngine>());

            var result = interpreter.Interpret(scen.ScriptSyntaxNodes[0], out var n);

            Assert.Equal(ScriptDataType.Boolean, result.DataType);
            Assert.False(result.Boolean);
            Assert.Equal(ushort.MaxValue, n);
        }

        [Fact]
        public void Interpret_LessThanFloat()
        {
            var scen = new ScenarioTag(0)
            {
                ScriptSyntaxNodes = new ScenarioTag.ScriptSyntaxNode[]
                {
                    ScopeNode(ScriptDataType.Boolean, op: 5, child: 1),
                        Node(NodeType.Expression, ScriptDataType.MethodOrOperator, op: ScriptOps.LessThan, next: 2),
                            Node(NodeType.Expression, ScriptDataType.Float, op: 6, data: From(1), next: 3),
                                Node(NodeType.Expression, ScriptDataType.Float, op: 6, data: From(2))
                }
            };

            var interpreter = new ScriptInterpreter(scen, Mock.Of<IScriptEngine>());

            var result = interpreter.Interpret(scen.ScriptSyntaxNodes[0], out var n);

            Assert.Equal(ScriptDataType.Boolean, result.DataType);
            Assert.True(result.Boolean);
            Assert.Equal(ushort.MaxValue, n);
        }

        [Fact]
        public void Interpret_NotLessThanFloat()
        {
            var scen = new ScenarioTag(0)
            {
                ScriptSyntaxNodes = new ScenarioTag.ScriptSyntaxNode[]
                {
                    ScopeNode(ScriptDataType.Boolean, op: 5, child: 1),
                        Node(NodeType.Expression, ScriptDataType.MethodOrOperator, op: ScriptOps.LessThan, next: 2),
                            Node(NodeType.Expression, ScriptDataType.Float, op: 6, data: From(2), next: 3),
                                Node(NodeType.Expression, ScriptDataType.Float, op: 6, data: From(1))
                }
            };

            var interpreter = new ScriptInterpreter(scen, Mock.Of<IScriptEngine>());

            var result = interpreter.Interpret(scen.ScriptSyntaxNodes[0], out var n);

            Assert.Equal(ScriptDataType.Boolean, result.DataType);
            Assert.False(result.Boolean);
            Assert.Equal(ushort.MaxValue, n);
        }

        [Fact]
        public void Interpret_LessThanOrEqualFloat()
        {
            var scen = new ScenarioTag(0)
            {
                ScriptSyntaxNodes = new ScenarioTag.ScriptSyntaxNode[]
                {
                    ScopeNode(ScriptDataType.Boolean, op: 5, child: 1),
                        Node(NodeType.Expression, ScriptDataType.MethodOrOperator, op: ScriptOps.LessThanOrEqual, next: 2),
                            Node(NodeType.Expression, ScriptDataType.Float, op: 6, data: From(2), next: 3),
                                Node(NodeType.Expression, ScriptDataType.Float, op: 6, data: From(2))
                }
            };

            var interpreter = new ScriptInterpreter(scen, Mock.Of<IScriptEngine>());

            var result = interpreter.Interpret(scen.ScriptSyntaxNodes[0], out var n);

            Assert.Equal(ScriptDataType.Boolean, result.DataType);
            Assert.True(result.Boolean);
            Assert.Equal(ushort.MaxValue, n);
        }

        [Fact]
        public void Interpret_NotLessThanOrEqualFloat()
        {
            var scen = new ScenarioTag(0)
            {
                ScriptSyntaxNodes = new ScenarioTag.ScriptSyntaxNode[]
                {
                    ScopeNode(ScriptDataType.Boolean, op: 5, child: 1),
                        Node(NodeType.Expression, ScriptDataType.MethodOrOperator, op: ScriptOps.LessThanOrEqual, next: 2),
                            Node(NodeType.Expression, ScriptDataType.Float, op: 6, data: From(2), next: 3),
                                Node(NodeType.Expression, ScriptDataType.Float, op: 6, data: From(1))
                }
            };

            var interpreter = new ScriptInterpreter(scen, Mock.Of<IScriptEngine>());

            var result = interpreter.Interpret(scen.ScriptSyntaxNodes[0], out var n);

            Assert.Equal(ScriptDataType.Boolean, result.DataType);
            Assert.False(result.Boolean);
            Assert.Equal(ushort.MaxValue, n);
        }

        [Fact]
        public void Interpret_GreaterThanFloat()
        {
            var scen = new ScenarioTag(0)
            {
                ScriptSyntaxNodes = new ScenarioTag.ScriptSyntaxNode[]
                {
                    ScopeNode(ScriptDataType.Boolean, op: 5, child: 1),
                        Node(NodeType.Expression, ScriptDataType.MethodOrOperator, op: ScriptOps.GreaterThan, next: 2),
                            Node(NodeType.Expression, ScriptDataType.Float, op: 6, data: From(2), next: 3),
                                Node(NodeType.Expression, ScriptDataType.Float, op: 6, data: From(1))
                }
            };

            var interpreter = new ScriptInterpreter(scen, Mock.Of<IScriptEngine>());

            var result = interpreter.Interpret(scen.ScriptSyntaxNodes[0], out var n);

            Assert.Equal(ScriptDataType.Boolean, result.DataType);
            Assert.True(result.Boolean);
            Assert.Equal(ushort.MaxValue, n);
        }

        [Fact]
        public void Interpret_NotGreaterThanFloat()
        {
            var scen = new ScenarioTag(0)
            {
                ScriptSyntaxNodes = new ScenarioTag.ScriptSyntaxNode[]
                {
                    ScopeNode(ScriptDataType.Boolean, op: 5, child: 1),
                        Node(NodeType.Expression, ScriptDataType.MethodOrOperator, op: ScriptOps.GreaterThan, next: 2),
                            Node(NodeType.Expression, ScriptDataType.Float, op: 6, data: From(1), next: 3),
                                Node(NodeType.Expression, ScriptDataType.Float, op: 6, data: From(2))
                }
            };

            var interpreter = new ScriptInterpreter(scen, Mock.Of<IScriptEngine>());

            var result = interpreter.Interpret(scen.ScriptSyntaxNodes[0], out var n);

            Assert.Equal(ScriptDataType.Boolean, result.DataType);
            Assert.False(result.Boolean);
            Assert.Equal(ushort.MaxValue, n);
        }

        [Fact]
        public void Interpret_GreaterThanOrEqualFloat()
        {
            var scen = new ScenarioTag(0)
            {
                ScriptSyntaxNodes = new ScenarioTag.ScriptSyntaxNode[]
                {
                    ScopeNode(ScriptDataType.Boolean, op: 5, child: 1),
                        Node(NodeType.Expression, ScriptDataType.MethodOrOperator, op: ScriptOps.GreaterThanOrEqual, next: 2),
                            Node(NodeType.Expression, ScriptDataType.Float, op: 6, data: From(2), next: 3),
                                Node(NodeType.Expression, ScriptDataType.Float, op: 6, data: From(2))
                }
            };

            var interpreter = new ScriptInterpreter(scen, Mock.Of<IScriptEngine>());

            var result = interpreter.Interpret(scen.ScriptSyntaxNodes[0], out var n);

            Assert.Equal(ScriptDataType.Boolean, result.DataType);
            Assert.True(result.Boolean);
            Assert.Equal(ushort.MaxValue, n);
        }

        [Fact]
        public void Interpret_NotGreaterThanOrEqualFloat()
        {
            var scen = new ScenarioTag(0)
            {
                ScriptSyntaxNodes = new ScenarioTag.ScriptSyntaxNode[]
                {
                    ScopeNode(ScriptDataType.Boolean, op: 5, child: 1),
                        Node(NodeType.Expression, ScriptDataType.MethodOrOperator, op: ScriptOps.GreaterThanOrEqual, next: 2),
                            Node(NodeType.Expression, ScriptDataType.Float, op: 6, data: From(1), next: 3),
                                Node(NodeType.Expression, ScriptDataType.Float, op: 6, data: From(2))
                }
            };

            var interpreter = new ScriptInterpreter(scen, Mock.Of<IScriptEngine>());

            var result = interpreter.Interpret(scen.ScriptSyntaxNodes[0], out var n);

            Assert.Equal(ScriptDataType.Boolean, result.DataType);
            Assert.False(result.Boolean);
            Assert.Equal(ushort.MaxValue, n);
        }

        [Fact]
        public void Interpret_And()
        {
            var scen = new ScenarioTag(0)
            {
                ScriptSyntaxNodes = new ScenarioTag.ScriptSyntaxNode[]
                {
                    ScopeNode(ScriptDataType.Boolean, op: 5, child: 1),
                        Node(NodeType.Expression, ScriptDataType.MethodOrOperator, op: ScriptOps.And, next: 2),
                            Node(NodeType.Expression, ScriptDataType.Boolean, op: 5, data: 1, next: 3),
                                Node(NodeType.Expression, ScriptDataType.Boolean, op: 5, data: 1, next: 4),
                                    Node(NodeType.Expression, ScriptDataType.Boolean, op: 5, data: 1),
                }
            };

            var interpreter = new ScriptInterpreter(scen, Mock.Of<IScriptEngine>());

            var result = interpreter.Interpret(scen.ScriptSyntaxNodes[0], out var n);

            Assert.Equal(ScriptDataType.Boolean, result.DataType);
            Assert.True(result.Boolean);
            Assert.Equal(ushort.MaxValue, n);
        }

        [Fact]
        public void Interpret_NotAnd()
        {
            var scen = new ScenarioTag(0)
            {
                ScriptSyntaxNodes = new ScenarioTag.ScriptSyntaxNode[]
                {
                    ScopeNode(ScriptDataType.Boolean, op: 5, child: 1),
                        Node(NodeType.Expression, ScriptDataType.MethodOrOperator, op: ScriptOps.And, next: 2),
                            Node(NodeType.Expression, ScriptDataType.Boolean, op: 5, data: 1, next: 3),
                                Node(NodeType.Expression, ScriptDataType.Boolean, op: 5, data: 1, next: 4),
                                    Node(NodeType.Expression, ScriptDataType.Boolean, op: 5, data: 0),
                }
            };

            var interpreter = new ScriptInterpreter(scen, Mock.Of<IScriptEngine>());

            var result = interpreter.Interpret(scen.ScriptSyntaxNodes[0], out var n);

            Assert.Equal(ScriptDataType.Boolean, result.DataType);
            Assert.False(result.Boolean);
            Assert.Equal(ushort.MaxValue, n);
        }

        [Fact]
        public void Interpret_OrAll()
        {
            var scen = new ScenarioTag(0)
            {
                ScriptSyntaxNodes = new ScenarioTag.ScriptSyntaxNode[]
                {
                    ScopeNode(ScriptDataType.Boolean, op: 5, child: 1),
                        Node(NodeType.Expression, ScriptDataType.MethodOrOperator, op: ScriptOps.Or, next: 2),
                            Node(NodeType.Expression, ScriptDataType.Boolean, op: 5, data: 1, next: 3),
                                Node(NodeType.Expression, ScriptDataType.Boolean, op: 5, data: 1, next: 4),
                                    Node(NodeType.Expression, ScriptDataType.Boolean, op: 5, data: 1),
                }
            };

            var interpreter = new ScriptInterpreter(scen, Mock.Of<IScriptEngine>());

            var result = interpreter.Interpret(scen.ScriptSyntaxNodes[0], out var n);

            Assert.Equal(ScriptDataType.Boolean, result.DataType);
            Assert.True(result.Boolean);
            Assert.Equal(ushort.MaxValue, n);
        }

        [Fact]
        public void Interpret_OrAny()
        {
            var scen = new ScenarioTag(0)
            {
                ScriptSyntaxNodes = new ScenarioTag.ScriptSyntaxNode[]
                {
                    ScopeNode(ScriptDataType.Boolean, op: 5, child: 1),
                        Node(NodeType.Expression, ScriptDataType.MethodOrOperator, op: ScriptOps.Or, next: 2),
                            Node(NodeType.Expression, ScriptDataType.Boolean, op: 5, data: 0, next: 3),
                                Node(NodeType.Expression, ScriptDataType.Boolean, op: 5, data: 1, next: 4),
                                    Node(NodeType.Expression, ScriptDataType.Boolean, op: 5, data: 0),
                }
            };

            var interpreter = new ScriptInterpreter(scen, Mock.Of<IScriptEngine>());

            var result = interpreter.Interpret(scen.ScriptSyntaxNodes[0], out var n);

            Assert.Equal(ScriptDataType.Boolean, result.DataType);
            Assert.True(result.Boolean);
            Assert.Equal(ushort.MaxValue, n);
        }

        [Fact]
        public void Interpret_NotOr()
        {
            var scen = new ScenarioTag(0)
            {
                ScriptSyntaxNodes = new ScenarioTag.ScriptSyntaxNode[]
                {
                    ScopeNode(ScriptDataType.Boolean, op: 5, child: 1),
                        Node(NodeType.Expression, ScriptDataType.MethodOrOperator, op: ScriptOps.Or, next: 2),
                            Node(NodeType.Expression, ScriptDataType.Boolean, op: 5, data: 0, next: 3),
                                Node(NodeType.Expression, ScriptDataType.Boolean, op: 5, data: 0, next: 4),
                                    Node(NodeType.Expression, ScriptDataType.Boolean, op: 5, data: 0),
                }
            };

            var interpreter = new ScriptInterpreter(scen, Mock.Of<IScriptEngine>());

            var result = interpreter.Interpret(scen.ScriptSyntaxNodes[0], out var n);

            Assert.Equal(ScriptDataType.Boolean, result.DataType);
            Assert.False(result.Boolean);
            Assert.Equal(ushort.MaxValue, n);
        }

        [Fact]
        public void Interpret_NotA()
        {
            var scen = new ScenarioTag(0)
            {
                ScriptSyntaxNodes = new ScenarioTag.ScriptSyntaxNode[]
                {
                    ScopeNode(ScriptDataType.Boolean, op: 5, child: 1),
                        Node(NodeType.Expression, ScriptDataType.MethodOrOperator, op: ScriptOps.Not, next: 2),
                            Node(NodeType.Expression, ScriptDataType.Boolean, op: 5, data: 1)
                }
            };

            var interpreter = new ScriptInterpreter(scen, Mock.Of<IScriptEngine>());

            var result = interpreter.Interpret(scen.ScriptSyntaxNodes[0], out var n);

            Assert.Equal(ScriptDataType.Boolean, result.DataType);
            Assert.False(result.Boolean);
            Assert.Equal(ushort.MaxValue, n);
        }

        [Fact]
        public void Interpret_NotB()
        {
            var scen = new ScenarioTag(0)
            {
                ScriptSyntaxNodes = new ScenarioTag.ScriptSyntaxNode[]
                {
                    ScopeNode(ScriptDataType.Boolean, op: 5, child: 1),
                        Node(NodeType.Expression, ScriptDataType.MethodOrOperator, op: ScriptOps.Not, next: 2),
                            Node(NodeType.Expression, ScriptDataType.Boolean, op: 5, data: 0)
                }
            };

            var interpreter = new ScriptInterpreter(scen, Mock.Of<IScriptEngine>());

            var result = interpreter.Interpret(scen.ScriptSyntaxNodes[0], out var n);

            Assert.Equal(ScriptDataType.Boolean, result.DataType);
            Assert.True(result.Boolean);
            Assert.Equal(ushort.MaxValue, n);
        }

        private ScenarioTag.ScriptSyntaxNode ScopeNode(ScriptDataType dt, ushort op, ushort child, ushort next = 65535)
        {
            return Node(NodeType.Scope, dt, op, next, child);
        }

        private ScenarioTag.ScriptSyntaxNode Node(NodeType nt, ScriptDataType dt, ushort op, ushort next = 65535, uint data = 0)
        {
            return new ScenarioTag.ScriptSyntaxNode()
            {
                NodeType = nt,
                DataType = dt,
                NextIndex = next,
                OperationId = op,
                NodeData_32 = data
            };
        }

        private static uint From(float f)
        {
            return (uint)BitConverter.SingleToInt32Bits(f);
        }
    }
}
