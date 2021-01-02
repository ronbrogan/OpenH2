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
                CreateState(variable.Value_H16, out var state);
                var completed = Interpret(ref state);
                Debug.Assert(completed);
                variables[i] = state.Result;
            }
        }

        /// <summary>
        /// Entry point to setup the interpreter state/callstack with the provided node
        /// </summary>
        public void CreateState(int nodeIndex, out State state)
        {
            state = State.Create();

            var node = this.scenario.ScriptSyntaxNodes[nodeIndex];

            if(node.NodeType == NodeType.BuiltinInvocation || node.NodeType == NodeType.ScriptInvocation)
            {
                // Push node as normal
                state.Push(new StackFrame()
                {
                    Locals = new(6),
                    OriginatingNode = node,
                    Previous = this.scenario.ScriptSyntaxNodes[node.NodeData_H16],
                    Next = ushort.MaxValue
                });
            }
            else
            {
                // Create synthetic frame and set next?
            }
        }

        public bool Interpret(int nodeIndex, out State state)
        {
            CreateState(nodeIndex, out state);

            while (state.FrameCount > 0 && state.Yield == false)
            {
                Interpret(ref state.TopFrame, ref state);
            }

            return true;
        }

        /// <summary>
        /// Method that will be called repeatedly
        /// Will block until the callstack terminates or yields for continuation
        /// </summary>
        /// <returns>True if completed, false if yielded for continuation. Caller should eventually call again if return value is false</returns>
        public bool Interpret(ref State state)
        {
            while(state.Yield == false)
            {
                Interpret(ref state.TopFrame, ref state);
            }

            return true;
        }

        private void Interpret(ref StackFrame topFrame, ref State state)
        {
            // Always defer interpretation to the invocation that created the frame, it knows what to do
            InterpretFrame(topFrame.OriginatingNode, ref state);
        }

        private void InterpretFrame(ScenarioTag.ScriptSyntaxNode node, ref State state)
        {
            switch (node.NodeType)
            {
                case NodeType.BuiltinInvocation: InterpretBuiltinFrame(node, ref state); break;
                case NodeType.ScriptInvocation: InterpretScriptFrame(node, ref state); break;
                default: throw new ("Non-invocation node provided to InterpretInvocation");
            };
        }

        private void InterpretBuiltinFrame(ScenarioTag.ScriptSyntaxNode node, ref State state)
        {
            ref var top = ref state.TopFrame;
            if (top.Next != ushort.MaxValue)
            {
                // Grab next node, reset index to allow implementation to dictate arg evaluation
                var nextNode = this.scenario.ScriptSyntaxNodes[top.Next];
                top.Next = ushort.MaxValue;
                Interpret(nextNode, ref state);
            }
            else
            {
                // Need to defer to implementation, allowing implementation to indicate that arguments are needed

                InterpretBuiltinInvocation(node, ref state);

                // If implementation terminates, we need to pop current frame and push result onto next frame's Locals?
                // If we pop frame and push to locals, how do we handle the bottom-most frame?
            }
        }

        private void InterpretScriptFrame(ScenarioTag.ScriptSyntaxNode node, ref State state)
        {
            // Need to defer to implementation, pre-gather all arguments? even support arguments for this?

            // If implementation terminates, we need to pop current frame and push result onto next frame's Locals?
            // If we pop frame and push to locals, how do we handle the bottom-most frame?
        }

        private void Interpret(ScenarioTag.ScriptSyntaxNode node, ref State state)
        {
            switch(node.NodeType)
            {
                case NodeType.BuiltinInvocation: PushBuiltinInvocation(node, ref state); break;
                //case NodeType.ScriptInvocation: PushScriptInvocation(node, ref state); break;
                case NodeType.Expression: InterpretExpression(node, ref state); break;
                case NodeType.VariableAccess: InterpretVariableAccess(node, ref state); break;
                default: throw new NotSupportedException($"NodeType {node.NodeType} is not supported");
            };
        }

        private void PushBuiltinInvocation(ScenarioTag.ScriptSyntaxNode node, ref State state)
        {
            Debug.Assert(node.NodeType == NodeType.BuiltinInvocation);

            state.Push(new StackFrame()
            {
                Locals = new(6),
                OriginatingNode = node,
                Previous = this.scenario.ScriptSyntaxNodes[node.NodeData_H16],
                Next = ushort.MaxValue
            });
        }

        private void InterpretExpression(ScenarioTag.ScriptSyntaxNode node, ref State state)
        {
            Debug.Assert(node.DataType != ScriptDataType.MethodOrOperator, "MethodOrOperator should be handled by wrapping node");

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

            ref var frame = ref state.TopFrame;
            frame.Locals.Push(result);
            frame.Previous = node;
        }

        private void InterpretVariableAccess(ScenarioTag.ScriptSyntaxNode node, ref State state)
        {
            // TODO, handle GET and SET cases
        }

        private void InterpretBuiltinInvocation(ScenarioTag.ScriptSyntaxNode node, ref State state)
        {
            // Dispatch to relevant implementation

            // Handle methods that special-case arguments
            switch(node.OperationId)
            {
                //case ScriptOps.Begin: this.Begin(node, ref state); return;
                //case ScriptOps.BeginRandom: this.BeginRandom(node, ref state); return;
                //case ScriptOps.If: this.If(node, ref state); return;
                //case ScriptOps.And: this.And(node); return;
                //case ScriptOps.Or: this.Or(node); return;
                //case ScriptOps.Set: this.Set(node); return;
                default: break;
            };

            // Handle methods that consume all arguments
            var argsRemain = PrepareNextArgument(ref state);
            if(argsRemain)
            {
                return;
            }

            switch(node.OperationId)
            {
                case ScriptOps.Not: this.Not(node, ref state); break;
                //case ScriptOps.Equals: this.ValueEquals(node); break;
                //case ScriptOps.GreaterThan: this.GreaterThan(node); break;
                //case ScriptOps.LessThan: this.LessThan(node); break;
                //case ScriptOps.GreaterThanOrEqual: this.GreaterThanOrEquals(node); break;
                //case ScriptOps.LessThanOrEqual: this.LessThanOrEquals(node); break;

                //case ScriptOps.Add: this.Add(node); break;
                //case ScriptOps.Subtract: this.Subtract(node); break;
                //case ScriptOps.Multiply: this.Multiply(node); break;
                //case ScriptOps.Divide: this.Divide(node); break;
                //case ScriptOps.Min: this.Min(node); break;
                //case ScriptOps.Max: this.Max(node); break;

                default: throw new NotSupportedException($"Operation {node.OperationId} not supported");
                //_: this.DispatchMethod(node)
            }
        }

        //private Result Begin(ScenarioTag.ScriptSyntaxNode node, ref State state)
        //{
        //    next = node.NextIndex;
        //
        //    Result result = default;
        //
        //    while (interruptRequested == false && next != ushort.MaxValue)
        //    {
        //        result = Interpret(this.scenario.ScriptSyntaxNodes[next], out next);
        //    }
        //
        //    return result;
        //}
        //
        //// TODO: not random!
        //private void BeginRandom(ScenarioTag.ScriptSyntaxNode node, ref State state)
        //{
        //    Begin(node, ref state);
        //}
        //
        //private void If(ScenarioTag.ScriptSyntaxNode node, ref State state)
        //{
        //    var argNext = node.NextIndex;
        //
        //    var condition = Interpret(this.scenario.ScriptSyntaxNodes[argNext], ref state);
        //    Debug.Assert(condition.DataType == ScriptDataType.Boolean);
        //
        //    var trueExp = this.scenario.ScriptSyntaxNodes[argNext];
        //
        //    if (condition.Boolean)
        //    {
        //        return Interpret(trueExp, out _);
        //    }
        //    else
        //    {
        //        var falseExp = this.scenario.ScriptSyntaxNodes[trueExp.NextIndex];
        //        return Interpret(falseExp, out _);
        //    }
        //}

        private void Not(ScenarioTag.ScriptSyntaxNode node, ref State state)
        {
            ref var frame = ref state.TopFrame;

            Debug.Assert(frame.Locals.Count == 1);

            var operand = frame.Locals.Pop();
            Debug.Assert(operand.DataType == ScriptDataType.Boolean);

            ProduceResult(Result.From(!operand.Boolean), ref state);
        }

        private bool PrepareNextArgument(ref State state)
        {
            ref var top = ref state.TopFrame;
            if (top.Previous.NextIndex == ushort.MaxValue)
            {
                return false;
            }
            else
            {
                top.Next = top.Previous.NextIndex;
                return true;
            }
        }

        private void ProduceResult(InterpreterResult result, ref State state)
        {
            var completedFrame = state.Pop();

            if(state.FrameCount == 0)
            {
                state.Result = result;
            }
            else
            {
                state.TopFrame.Locals.Push(result);
            }
        }
    }
}
