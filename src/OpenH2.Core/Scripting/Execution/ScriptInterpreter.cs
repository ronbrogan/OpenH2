using OpenBlam.Core.Extensions;
using OpenH2.Core.Tags.Scenario;
using System;
using System.Diagnostics;
using Result = OpenH2.Core.Scripting.Execution.InterpreterResult;

namespace OpenH2.Core.Scripting.Execution
{
    public class ScriptInterpreter
    {
        private readonly ScenarioTag scenario;
        private readonly IScriptEngine scriptEngine;

        public ScriptInterpreter(ScenarioTag scenario, IScriptEngine scriptEngine)
        {
            this.scenario = scenario;
            this.scriptEngine = scriptEngine;
        }

        internal Result Interpret(ScenarioTag.ScriptSyntaxNode node, out ushort next)
        {
            next = ushort.MaxValue;

            switch (node.NodeType)
            {
                case NodeType.Scope: return InterpretScope(node, out next);
                case NodeType.Expression: return InterpretExpression(node, out next);
                case NodeType.ScriptInvocation:
                    break;
                case NodeType.VariableAccess:
                    break;
            }

            return default;
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
                ScriptDataType.Void => default,
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

        private Result InterpretMethodOrOperator(ScenarioTag.ScriptSyntaxNode node)
        {
            // Collect arguments by evaluating child nodes

            // Dispatch to relevant implementation
            return node.OperationId switch
            {
                0 => this.Begin(node),
                1 => this.BeginRandom(node),
                2 => this.If(node),
                4 => this.Set(node),
                5 => this.And(node),
                6 => this.Or(node),
                7 => this.Add(node),
                8 => this.Subtract(node),
                9 => this.Multiply(node),
                10 => this.Divide(node),
                11 => this.Min(node),
                12 => this.Max(node),
                13 => this.ValueEquals(node),
                15 => this.GreaterThan(node),
                16 => this.LessThan(node),
                17 => this.GreaterThanOrEquals(node),
                18 => this.LessThanOrEquals(node),
                _ => this.DispatchMethodOrOperator(node)
            };
        }

        private Result Begin(ScenarioTag.ScriptSyntaxNode node)
        {
            return default;
        }

        private Result BeginRandom(ScenarioTag.ScriptSyntaxNode node)
        {
            return default;
        }

        private Result If(ScenarioTag.ScriptSyntaxNode node)
        {
            return default;
        }

        private Result Set(ScenarioTag.ScriptSyntaxNode node)
        {
            return default;
        }

        private Result And(ScenarioTag.ScriptSyntaxNode node)
        {
            return default;
        }

        private Result Or(ScenarioTag.ScriptSyntaxNode node)
        {
            return default;
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

            var firstOp = Interpret(this.scenario.ScriptSyntaxNodes[argNext], out argNext);
            var operand = Interpret(this.scenario.ScriptSyntaxNodes[argNext], out argNext);

            firstOp = Result.Min(firstOp, operand);

            Debug.Assert(argNext == ushort.MaxValue);

            return firstOp;
        }

        private Result GreaterThan(ScenarioTag.ScriptSyntaxNode node)
        {
            return default;
        }

        private Result GreaterThanOrEquals(ScenarioTag.ScriptSyntaxNode node)
        {
            return default;
        }

        private Result LessThan(ScenarioTag.ScriptSyntaxNode node)
        {
            return default;
        }

        private Result LessThanOrEquals(ScenarioTag.ScriptSyntaxNode node)
        {
            return default;
        }

        /*
         * Source generator for each IScriptEngine method that creates an Invoke_{Method} that
         * does {number of arguments} Interpret(nextNode) to collect arguments and finally calls the 
         * IScriptEngine instance method with the arguments. Value should be returned normally?
         * Then this DispatchMethodOrOperator method can be generated with a switch to each invoke, bypassing
         * the static dictionary altogether, removing array allocations for MethodInfo.Invoke and call overhead
         */
        private delegate Result DispatchMethodOrOperatorImpl(ScenarioTag.ScriptSyntaxNode node);
        private DispatchMethodOrOperatorImpl DispatchMethodOrOperator;

        private void Invoke_sleep(ScenarioTag.ScriptSyntaxNode node)
        {
            var argNext = node.NextIndex;

            var length = Interpret(this.scenario.ScriptSyntaxNodes[argNext], out argNext);
            Debug.Assert(argNext == ushort.MaxValue);

            this.scriptEngine.sleep((int)length);
        }
    }
}
