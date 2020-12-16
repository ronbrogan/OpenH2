using Moq;
using OpenH2.Core.Scripting;
using OpenH2.Core.Scripting.Execution;
using OpenH2.Core.Tags.Scenario;
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
                    Node(NodeType.Expression, ScriptDataType.Float, op: (ushort)ScriptDataType.Float, data: 1073741824),
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
                    Node(NodeType.Expression, ScriptDataType.Float, op: 6, data: 1073741824),
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
                            Node(NodeType.Expression, ScriptDataType.Float, op: 6, data: 1073741824, next: 3),
                                Node(NodeType.Expression, ScriptDataType.Float, op: 6, data: 1073741824, next: 4),
                                    Node(NodeType.Expression, ScriptDataType.Float, op: 6, data: 1073741824),
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
                            Node(NodeType.Expression, ScriptDataType.Float, op: 6, data: 1073741824, next: 3),
                                Node(NodeType.Expression, ScriptDataType.Float, op: 6, data: 1073741824, next: 4),
                                    Node(NodeType.Expression, ScriptDataType.Float, op: 6, data: 1073741824),
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
                            Node(NodeType.Expression, ScriptDataType.Float, op: 6, data: 1073741824, next: 3),
                                Node(NodeType.Expression, ScriptDataType.Float, op: 6, data: 1073741824)
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
                            Node(NodeType.Expression, ScriptDataType.Float, op: 6, data: 1073741824, next: 3),
                                Node(NodeType.Expression, ScriptDataType.Float, op: 6, data: 1073741824, next: 4),
                                    Node(NodeType.Expression, ScriptDataType.Float, op: 6, data: 1073741824),
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
                            Node(NodeType.Expression, ScriptDataType.Float, op: 6, data: 1073741824, next: 3),
                                Node(NodeType.Expression, ScriptDataType.Float, op: 6, data: 1073741824)
                }
            };

            var interpreter = new ScriptInterpreter(scen, Mock.Of<IScriptEngine>());

            var result = interpreter.Interpret(scen.ScriptSyntaxNodes[0], out var n);

            Assert.Equal(ScriptDataType.Float, result.DataType);
            Assert.Equal(1f, result.Float);
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
    }
}
