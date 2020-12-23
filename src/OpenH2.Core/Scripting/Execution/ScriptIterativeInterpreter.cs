using OpenBlam.Core.Extensions;
using OpenH2.Core.Exceptions;
using OpenH2.Core.Tags.Scenario;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Result = OpenH2.Core.Scripting.Execution.InterpreterResult;
using State = OpenH2.Core.Scripting.Execution.InterpreterState;

namespace OpenH2.Core.Scripting.Execution
{
    /// <summary>
    /// The primary goals of this are: 1) No heap allocations to avoid GC and 2) support continuations
    /// </summary>
    public class ScriptIterativeInterpreter
    {
        private readonly ScenarioTag scenario;
        private readonly IScriptEngine scriptEngine;
        private readonly Result[] variables;

        public ScriptIterativeInterpreter(
            ScenarioTag scenario,
            IScriptEngine scriptEngine)
        {
            this.scenario = scenario;
            this.scriptEngine = scriptEngine;

            if (scenario.ScriptVariables?.Length > 0)
            {
                this.variables = new Result[scenario.ScriptVariables.Length];

                InitializeVariables();
            }
            else
            {
                this.variables = Array.Empty<Result>();
            }
        }

        public Result GetVariable(int index)
        {
            return this.variables[index];
        }

        private void InitializeVariables()
        {
            for (int i = 0; i < this.scenario.ScriptVariables.Length; i++)
            {
                var variable = this.scenario.ScriptVariables[i];

                var completed = Interpret(this.scenario.ScriptSyntaxNodes[variable.Value_H16], out var state);
                Debug.Assert(completed);
                variables[i] = state.Results.Pop();
            }
        }

        public bool Interpret(ScenarioTag.ScriptSyntaxNode node, out State state)
        {
            state = State.Create();

            Push(node, ref state);

            return Interpret(ref state);
        }

        public bool Interpret(ref State state)
        {
            while(state.CallStack.TryPop(out var node))
            {
                bool ret = node.NodeType switch
                {
                    NodeType.Scope => InterpretScope(node, ref state),
                    NodeType.Expression => InterpretExpression(node, ref state),
                    NodeType.ScriptInvocation => InterpretScriptInvocation(node, ref state),
                    NodeType.VariableAccess => InterpretVariableAccess(node, ref state),
                    _ => default,
                };
            }

            return true;
        }

        private void Push(ScenarioTag.ScriptSyntaxNode node, ref State state)
        {
            bool ret = node.NodeType switch
            {
                NodeType.Scope => PushScope(node, ref state),
                NodeType.Expression => PushExpression(node, ref state),
                NodeType.ScriptInvocation => PushScriptInvocation(node, ref state),
                NodeType.VariableAccess => PushVariableAccess(node, ref state),
                _ => default,
            };
        }

        private bool PushScope(ScenarioTag.ScriptSyntaxNode node, ref State state)
        {
            state.CallStack.Push(node);

            Debug.Assert(node.NodeData_H16 != ushort.MaxValue);
            Push(this.scenario.ScriptSyntaxNodes[node.NodeData_H16], ref state);

            return true;
        }

        private bool PushExpression(ScenarioTag.ScriptSyntaxNode node, ref State state)
        {
            state.CallStack.Push(node);

            if(node.NextIndex != ushort.MaxValue)
            {
                Push(this.scenario.ScriptSyntaxNodes[node.NextIndex], ref state);
            }
            
            return true;
        }

        private bool PushScriptInvocation(ScenarioTag.ScriptSyntaxNode node, ref State state)
        {
            state.CallStack.Push(node);

            Debug.Assert(node.NodeData_H16 != ushort.MaxValue);
            //Push(this.scenario.ScriptSyntaxNodes[node.NodeData_H16], ref state);

            return true;
        }

        private bool PushVariableAccess(ScenarioTag.ScriptSyntaxNode node, ref State state)
        {
            state.CallStack.Push(node);
            return true;
        }

        /// <summary>
        /// Pop scope is responsible for preparing its return value and pushing its sibling onto the stack
        /// </summary>
        /// <param name="node"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        private bool InterpretScope(ScenarioTag.ScriptSyntaxNode node, ref State state)
        {
            var destType = node.DataType;

            if (destType != ScriptDataType.Void)
            {
                var value = state.Results.Pop();

                if (value.DataType != destType)
                {
                    switch (destType)
                    {
                        case ScriptDataType.Float:
                            value.Float = value.GetFloat();
                            value.DataType = ScriptDataType.Float;
                            break;
                        case ScriptDataType.Int:
                            value.Int = value.GetInt();
                            value.DataType = ScriptDataType.Int;
                            break;
                        case ScriptDataType.Short:
                            value.Short = value.GetShort();
                            value.DataType = ScriptDataType.Short;
                            break;
                        default:
                            Debug.Fail($"No configured cast from {value.DataType} to {destType}");
                            break;
                    }
                }

                state.Results.Push(value);
            }

            if (node.NextIndex != ushort.MaxValue)
            {
                Push(this.scenario.ScriptSyntaxNodes[node.NextIndex], ref state);
            }

            return true;
        }

        private bool InterpretScriptInvocation(ScenarioTag.ScriptSyntaxNode node, ref State state)
        {
            var destType = node.DataType;

            var invocationNode = this.scenario.ScriptSyntaxNodes[node.NodeData_H16];
            Debug.Assert(invocationNode.NodeType == NodeType.Expression);
            Debug.Assert(invocationNode.DataType == ScriptDataType.MethodOrOperator);
            Debug.Assert(invocationNode.NextCheckval == ushort.MaxValue);

            var methodToInvoke = this.scenario.ScriptMethods[invocationNode.OperationId];

            // TODO: how to handle invoking other methods?
            //var value = Interpret(this.scenario.ScriptSyntaxNodes[methodToInvoke.SyntaxNodeIndex], out next);
            var value = default(Result);

            if (value.DataType != destType)
            {
                switch (destType)
                {
                    case ScriptDataType.Float:
                        value.Float = value.GetFloat();
                        value.DataType = ScriptDataType.Float;
                        break;
                    case ScriptDataType.Int:
                        value.Int = value.GetInt();
                        value.DataType = ScriptDataType.Int;
                        break;
                    case ScriptDataType.Short:
                        value.Short = value.GetShort();
                        value.DataType = ScriptDataType.Short;
                        break;
                    default:
                        Debug.Fail($"No configured cast from {value.DataType} to {destType}");
                        break;
                }
            }

            if (value.DataType != ScriptDataType.Void)
            {
                state.Results.Push(value);
            }

            if (node.NextIndex != ushort.MaxValue)
            {
                Push(this.scenario.ScriptSyntaxNodes[node.NextIndex], ref state);
            }

            return true;
        }

        private bool InterpretVariableAccess(ScenarioTag.ScriptSyntaxNode node, ref State state)
        {
            state.Results.Push(this.variables[node.NodeData_H16]);
            return true;
        }

        /// <summary>
        /// Interprets an expression node, producing an arbitrary value (void, value, or ref) and returns the next node index to execute
        /// </summary>
        private bool InterpretExpression(ScenarioTag.ScriptSyntaxNode node, ref State state)
        {
            if (node.DataType == ScriptDataType.MethodOrOperator)
            {
                return InterpretMethodOrOperator(node, ref state);
            }

            Result result = node.DataType switch
            {
                ScriptDataType.Void => Result.From(),
                ScriptDataType.Boolean => Result.From(node.NodeData_B3 == 1 ? true : false),
                ScriptDataType.Float => Result.From(node.NodeData_32),
                ScriptDataType.Short => Result.From(node.NodeData_H16),
                ScriptDataType.Int => Result.From(node.NodeData_32),
                ScriptDataType.VehicleSeat => new Result(),
                ScriptDataType.String => Result.From(SpanByteExtensions.ReadStringStarting(this.scenario.ScriptStrings, node.NodeString)),
                ScriptDataType.ScriptReference => new Result(),
                ScriptDataType.StringId => new Result(),
                ScriptDataType.Trigger => Result.From(this.scenario.TriggerVolumes[node.NodeData_H16].GameObject),
                ScriptDataType.LocationFlag => Result.From(this.scenario.LocationFlagDefinitions[node.NodeData_H16].GameObject),
                ScriptDataType.CameraPathTarget => new Result(),
                ScriptDataType.CinematicTitle => new Result(),
                ScriptDataType.DeviceGroup => new Result(),
                ScriptDataType.AI => new Result(),
                ScriptDataType.AIScript => new Result(),
                ScriptDataType.AIBehavior => new Result(),
                ScriptDataType.AIOrders => Result.From(this.scenario.AiOrderDefinitions[node.NodeData_H16].GameObject),
                ScriptDataType.StartingProfile => Result.From(this.scenario.StartingProfileDefinitions[node.NodeData_H16].GameObject),
                ScriptDataType.Bsp => new Result(),
                ScriptDataType.NavigationPoint => new Result(),
                ScriptDataType.SpatialPoint => new Result(),
                ScriptDataType.List => new Result(),
                ScriptDataType.Sound => new Result(),
                ScriptDataType.Effect => new Result(),
                ScriptDataType.DamageEffect => new Result(),
                ScriptDataType.LoopingSound => new Result(),
                ScriptDataType.TagReference => new Result(),
                ScriptDataType.Animation => new Result(),
                ScriptDataType.Model => new Result(),
                ScriptDataType.GameDifficulty => new Result(),
                ScriptDataType.Team => new Result(),
                ScriptDataType.DamageState => new Result(),
                ScriptDataType.Entity => new Result(),
                ScriptDataType.Unit => new Result(),
                ScriptDataType.Vehicle => new Result(),
                ScriptDataType.WeaponReference => new Result(),
                ScriptDataType.Device => new Result(),
                ScriptDataType.Scenery => new Result(),
                ScriptDataType.EntityIdentifier => new Result(),
                _ => throw new NotImplementedException(),
            };

            result.DataType = node.DataType;

            state.Results.Push(result);
            return true;
        }

        private bool InterpretMethodOrOperator(ScenarioTag.ScriptSyntaxNode node, ref State state)
        {
            // Dispatch to relevant implementation
            return node.OperationId switch
            {
                //ScriptOps.Begin => this.Begin(node, out next),
                //ScriptOps.BeginRandom => this.BeginRandom(node, out next),
                //ScriptOps.If => this.If(node),
                //ScriptOps.Set => this.Set(node),
                //ScriptOps.And => this.And(node),
                //ScriptOps.Or => this.Or(node),
                ScriptOps.Add => this.Add(node, ref state),
                //ScriptOps.Subtract => this.Subtract(node),
                //ScriptOps.Multiply => this.Multiply(node),
                //ScriptOps.Divide => this.Divide(node),
                //ScriptOps.Min => this.Min(node),
                //ScriptOps.Max => this.Max(node),
                ScriptOps.Equals => this.ValueEquals(node, ref state),
                //ScriptOps.GreaterThan => this.GreaterThan(node),
                //ScriptOps.LessThan => this.LessThan(node),
                //ScriptOps.GreaterThanOrEqual => this.GreaterThanOrEquals(node),
                //ScriptOps.LessThanOrEqual => this.LessThanOrEquals(node),
                //ScriptOps.Not => this.Not(node),

                //_ => this.DispatchMethod(node)
            };
        }

        private bool Add(ScenarioTag.ScriptSyntaxNode node, ref State state)
        {
            var left = state.Results.Pop();

            while(state.Results.TryPop(out var right))
            {
                left.Float += right.GetFloat();
            };

            state.Results.Push(left);
            return true;
        }

        private bool ValueEquals(ScenarioTag.ScriptSyntaxNode node, ref State state)
        {
            var left = state.Results.Pop();
            var right = state.Results.Pop();

            var result = left.DataType switch
            {
                ScriptDataType.Boolean => Result.From(left.Boolean == right.Boolean),
                ScriptDataType.Float => Result.From(left.GetFloat() == right.GetFloat()),
                ScriptDataType.Short => Result.From(left.GetShort() == right.GetShort()),
                ScriptDataType.Int => Result.From(left.GetInt() == right.GetInt()),
                ScriptDataType.GameDifficulty => Result.From(left.GetShort() == right.GetShort()),
                _ => throw new InterpreterException($"Equality comparison for {left.DataType} is not supported")
            };

            state.Results.Push(result);

            return true;
        }
    }
}
