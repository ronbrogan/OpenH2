using OpenBlam.Core.Extensions;
using OpenH2.Core.Exceptions;
using OpenH2.Core.Tags.Scenario;
using System;
using System.Diagnostics;
using Result = OpenH2.Core.Scripting.Execution.InterpreterResult;

namespace OpenH2.Core.Scripting.Execution
{
    public partial class ScriptInterpreter
    {
        
    }

    public partial class ScriptInterpreter
    {
        private readonly ScenarioTag scenario;
        private readonly IScriptEngine scriptEngine;
        private readonly Result[] variables;

        public ScriptInterpreter(ScenarioTag scenario, IScriptEngine scriptEngine)
        {
            this.scenario = scenario;
            this.scriptEngine = scriptEngine;

            if(scenario.ScriptVariables?.Length > 0)
            {
                this.variables = new Result[scenario.ScriptVariables.Length];

                InitializeVariables();
            }
        }

        private void InitializeVariables()
        {
            for (int i = 0; i < this.scenario.ScriptVariables.Length; i++)
            {
                var variable = this.scenario.ScriptVariables[i];

                variables[i] = Interpret(this.scenario.ScriptSyntaxNodes[variable.Value_H16], out var next);
                Debug.Assert(next == ushort.MaxValue);
            }
        }

        internal Result Interpret(ScenarioTag.ScriptSyntaxNode node, out ushort next)
        {
            next = ushort.MaxValue;

            return node.NodeType switch
            {
                NodeType.Scope => InterpretScope(node, out next),
                NodeType.Expression => InterpretExpression(node, out next),
                NodeType.ScriptInvocation => InterpretScriptInvocation(node, out next),
                NodeType.VariableAccess => this.variables[node.NodeData_H16],
                _ => default,
            };
        }

        internal Result GetVariable(int index)
        {
            return this.variables[index];
        }

        /// <summary>
        /// Interprets a scope node, producing an arbitrary value and returns the next node index to execute
        /// </summary>
        /// <param name="node"></param>
        private Result InterpretScope(ScenarioTag.ScriptSyntaxNode node, out ushort next)
        {
            next = node.NextIndex;
            var destType = node.DataType;

            Result value = default;
            ushort innerNext = node.NodeData_H16;

            while (innerNext != ushort.MaxValue)
            {
                value = Interpret(this.scenario.ScriptSyntaxNodes[node.NodeData_H16], out innerNext);
            }

            if(value.DataType != destType)
            {
                switch(destType)
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

            return value;
        }

        /// <summary>
        /// Interprets an expression node, producing an arbitrary value (void, value, or ref) and returns the next node index to execute
        /// </summary>
        private Result InterpretExpression(ScenarioTag.ScriptSyntaxNode node, out ushort next)
        {
            next = node.NextIndex;

            if(node.DataType == ScriptDataType.MethodOrOperator)
            {
                // MethodOrOperator will always consume all child nodes, no next node to bubble
                next = ushort.MaxValue;
                return InterpretMethodOrOperator(node);
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
            return result;
        }

        private Result InterpretScriptInvocation(ScenarioTag.ScriptSyntaxNode node, out ushort next)
        {
            var destType = node.DataType;

            var invocationNode = this.scenario.ScriptSyntaxNodes[node.NodeData_H16];
            Debug.Assert(invocationNode.NodeType == NodeType.Expression);
            Debug.Assert(invocationNode.DataType == ScriptDataType.MethodOrOperator);
            Debug.Assert(invocationNode.NextCheckval == ushort.MaxValue);

            var methodToInvoke = this.scenario.ScriptMethods[invocationNode.OperationId];

            var value = Interpret(this.scenario.ScriptSyntaxNodes[methodToInvoke.SyntaxNodeIndex], out next);
            Debug.Assert(next == ushort.MaxValue);

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

            next = node.NextIndex;

            return value;
        }

        private Result InterpretMethodOrOperator(ScenarioTag.ScriptSyntaxNode node)
        {
            // Dispatch to relevant implementation
            return node.OperationId switch
            {
                ScriptOps.Begin => this.Begin(node),
                ScriptOps.BeginRandom => this.BeginRandom(node),
                ScriptOps.If => this.If(node),
                ScriptOps.Set => this.Set(node),
                ScriptOps.And => this.And(node),
                ScriptOps.Or => this.Or(node),
                ScriptOps.Add => this.Add(node),
                ScriptOps.Subtract => this.Subtract(node),
                ScriptOps.Multiply => this.Multiply(node),
                ScriptOps.Divide => this.Divide(node),
                ScriptOps.Min => this.Min(node),
                ScriptOps.Max => this.Max(node),
                ScriptOps.Equals => this.ValueEquals(node),
                ScriptOps.GreaterThan => this.GreaterThan(node),
                ScriptOps.LessThan => this.LessThan(node),
                ScriptOps.GreaterThanOrEqual => this.GreaterThanOrEquals(node),
                ScriptOps.LessThanOrEqual => this.LessThanOrEquals(node),
                ScriptOps.Not => this.Not(node),

                _ => this.DispatchMethod(node)
            };
        }

        private Result Begin(ScenarioTag.ScriptSyntaxNode node)
        {
            var argNext = node.NextIndex;

            Result result = default;

            while (argNext != ushort.MaxValue)
            {
                result = Interpret(this.scenario.ScriptSyntaxNodes[argNext], out argNext);
            }

            return result;
        }

        // TODO: not random!
        private Result BeginRandom(ScenarioTag.ScriptSyntaxNode node)
        {
            return Begin(node);
        }

        private Result If(ScenarioTag.ScriptSyntaxNode node)
        {
            var argNext = node.NextIndex;

            var condition = Interpret(this.scenario.ScriptSyntaxNodes[argNext], out argNext);
            Debug.Assert(condition.DataType == ScriptDataType.Boolean);

            var trueExp = this.scenario.ScriptSyntaxNodes[argNext];

            if (condition.Boolean)
            {
                return Interpret(trueExp, out _);
            }
            else
            {
                var falseExp = this.scenario.ScriptSyntaxNodes[trueExp.NextIndex];
                return Interpret(falseExp, out _);
            }
        }

        private Result Set(ScenarioTag.ScriptSyntaxNode node)
        {
            var variableAccess = this.scenario.ScriptSyntaxNodes[node.NextIndex];
            Debug.Assert(variableAccess.NodeType == NodeType.VariableAccess);
            var valueExp = this.scenario.ScriptSyntaxNodes[variableAccess.NextIndex];

            var variable = this.variables[variableAccess.NodeData_H16];
            Debug.Assert(variable.DataType == valueExp.DataType);

            var value = Interpret(valueExp, out var argNext);
            Debug.Assert(argNext == ushort.MaxValue);
            Debug.Assert(value.DataType == variable.DataType);

            this.variables[variableAccess.NodeData_H16] = value;
            return Result.From();
        }

        private Result And(ScenarioTag.ScriptSyntaxNode node)
        {
            var argNext = node.NextIndex;

            var result = Result.From(true);

            while (argNext != ushort.MaxValue)
            {
                var operand = Interpret(this.scenario.ScriptSyntaxNodes[argNext], out argNext);
                Debug.Assert(operand.DataType == ScriptDataType.Boolean);
                if (operand.Boolean == false)
                {
                    result.Boolean = false;
                    break;
                }
            }

            return result;
        }

        private Result Or(ScenarioTag.ScriptSyntaxNode node)
        {
            var argNext = node.NextIndex;

            var result = Result.From(false);

            while (argNext != ushort.MaxValue)
            {
                var operand = Interpret(this.scenario.ScriptSyntaxNodes[argNext], out argNext);
                Debug.Assert(operand.DataType == ScriptDataType.Boolean);
                if (operand.Boolean)
                {
                    result.Boolean = true;
                    break;
                }
            }

            return result;
        }

        private Result Not(ScenarioTag.ScriptSyntaxNode node)
        {
            var argNext = node.NextIndex;

            var result = Result.From(false);

            var operand = Interpret(this.scenario.ScriptSyntaxNodes[argNext], out argNext);

            Debug.Assert(operand.DataType == ScriptDataType.Boolean);
            Debug.Assert(argNext == ushort.MaxValue);

            result.Boolean = !operand.Boolean;

            return result;
        }

        private Result Add(ScenarioTag.ScriptSyntaxNode node)
        {
            var argNext = node.NextIndex;

            var firstOp = Interpret(this.scenario.ScriptSyntaxNodes[argNext], out argNext);

            while (argNext != ushort.MaxValue)
            {
                var operand = Interpret(this.scenario.ScriptSyntaxNodes[argNext], out argNext);

                firstOp.Add(operand);
            }

            return firstOp;
        }

        private Result Subtract(ScenarioTag.ScriptSyntaxNode node)
        {
            var argNext = node.NextIndex;

            var firstOp = Interpret(this.scenario.ScriptSyntaxNodes[argNext], out argNext);

            while (argNext != ushort.MaxValue)
            {
                var operand = Interpret(this.scenario.ScriptSyntaxNodes[argNext], out argNext);

                firstOp.Subtract(operand);
            }

            return firstOp;
        }

        private Result Multiply(ScenarioTag.ScriptSyntaxNode node)
        {
            var argNext = node.NextIndex;

            var firstOp = Interpret(this.scenario.ScriptSyntaxNodes[argNext], out argNext);

            while (argNext != ushort.MaxValue)
            {
                var operand = Interpret(this.scenario.ScriptSyntaxNodes[argNext], out argNext);

                firstOp.Multiply(operand);
            }

            return firstOp;
        }

        private Result Divide(ScenarioTag.ScriptSyntaxNode node)
        {
            var argNext = node.NextIndex;

            var firstOp = Interpret(this.scenario.ScriptSyntaxNodes[argNext], out argNext);

            while (argNext != ushort.MaxValue)
            {
                var operand = Interpret(this.scenario.ScriptSyntaxNodes[argNext], out argNext);

                firstOp.Divide(operand);
            }

            return firstOp;
        }

        private Result Min(ScenarioTag.ScriptSyntaxNode node)
        {
            var argNext = node.NextIndex;

            var firstOp = Interpret(this.scenario.ScriptSyntaxNodes[argNext], out argNext);

            while (argNext != ushort.MaxValue)
            {
                var operand = Interpret(this.scenario.ScriptSyntaxNodes[argNext], out argNext);

                firstOp = Result.Min(firstOp, operand);
            }

            return firstOp;
        }

        private Result Max(ScenarioTag.ScriptSyntaxNode node)
        {
            var argNext = node.NextIndex;

            var firstOp = Interpret(this.scenario.ScriptSyntaxNodes[argNext], out argNext);

            while (argNext != ushort.MaxValue)
            {
                var operand = Interpret(this.scenario.ScriptSyntaxNodes[argNext], out argNext);

                firstOp = Result.Max(firstOp, operand);
            }

            return firstOp;
        }

        private Result ValueEquals(ScenarioTag.ScriptSyntaxNode node)
        {
            var argNext = node.NextIndex;

            var left = Interpret(this.scenario.ScriptSyntaxNodes[argNext], out argNext);
            var right = Interpret(this.scenario.ScriptSyntaxNodes[argNext], out argNext);
            Debug.Assert(argNext == ushort.MaxValue);

            return left.DataType switch
            {
                ScriptDataType.Boolean => Result.From(left.Boolean == right.Boolean),
                ScriptDataType.Float => Result.From(left.GetFloat() == right.GetFloat()),
                ScriptDataType.Short => Result.From(left.GetShort() == right.GetShort()),
                ScriptDataType.Int => Result.From(left.GetInt() == right.GetInt()),
                ScriptDataType.GameDifficulty => Result.From(left.GetShort() == right.GetShort()),
                _ => throw new InterpreterException($"Equality comparison for {left.DataType} is not supported")
            };
        }

        private Result GreaterThan(ScenarioTag.ScriptSyntaxNode node)
        {
            var argNext = node.NextIndex;

            var left = Interpret(this.scenario.ScriptSyntaxNodes[argNext], out argNext);
            var right = Interpret(this.scenario.ScriptSyntaxNodes[argNext], out argNext);
            Debug.Assert(argNext == ushort.MaxValue);

            return left.DataType switch
            {
                ScriptDataType.Float => Result.From(left.GetFloat() > right.GetFloat()),
                ScriptDataType.Short => Result.From(left.GetShort() > right.GetShort()),
                _ => throw new InterpreterException($"Equality comparison for {left.DataType} is not supported")
            };
        }

        private Result GreaterThanOrEquals(ScenarioTag.ScriptSyntaxNode node)
        {
            var argNext = node.NextIndex;

            var left = Interpret(this.scenario.ScriptSyntaxNodes[argNext], out argNext);
            var right = Interpret(this.scenario.ScriptSyntaxNodes[argNext], out argNext);
            Debug.Assert(argNext == ushort.MaxValue);

            return left.DataType switch
            {
                ScriptDataType.Float => Result.From(left.GetFloat() >= right.GetFloat()),
                ScriptDataType.Short => Result.From(left.GetShort() >= right.GetShort()),
                _ => throw new InterpreterException($"Equality comparison for {left.DataType} is not supported")
            };
        }

        private Result LessThan(ScenarioTag.ScriptSyntaxNode node)
        {
            var argNext = node.NextIndex;

            var left = Interpret(this.scenario.ScriptSyntaxNodes[argNext], out argNext);
            var right = Interpret(this.scenario.ScriptSyntaxNodes[argNext], out argNext);
            Debug.Assert(argNext == ushort.MaxValue);

            return left.DataType switch
            {
                ScriptDataType.Float => Result.From(left.GetFloat() < right.GetFloat()),
                ScriptDataType.Short => Result.From(left.GetShort() < right.GetShort()),
                _ => throw new InterpreterException($"Equality comparison for {left.DataType} is not supported")
            };
        }

        private Result LessThanOrEquals(ScenarioTag.ScriptSyntaxNode node)
        {
            var argNext = node.NextIndex;

            var left = Interpret(this.scenario.ScriptSyntaxNodes[argNext], out argNext);
            var right = Interpret(this.scenario.ScriptSyntaxNodes[argNext], out argNext);
            Debug.Assert(argNext == ushort.MaxValue);

            return left.DataType switch
            {
                ScriptDataType.Float => Result.From(left.GetFloat() <= right.GetFloat()),
                ScriptDataType.Short => Result.From(left.GetShort() <= right.GetShort()),
                _ => throw new InterpreterException($"Equality comparison for {left.DataType} is not supported")
            };
        }

        /*
         * Source generator for each IScriptEngine method that creates an Invoke_{Method} that
         * does {number of arguments} Interpret(nextNode) to collect arguments and finally calls the 
         * IScriptEngine instance method with the arguments. Value should be returned normally?
         * Then this DispatchMethodOrOperator method can be generated with a switch to each invoke, bypassing
         * the static dictionary altogether, removing array allocations for MethodInfo.Invoke and call overhead
         */
        private partial Result DispatchMethod(ScenarioTag.ScriptSyntaxNode node);
    }
}
