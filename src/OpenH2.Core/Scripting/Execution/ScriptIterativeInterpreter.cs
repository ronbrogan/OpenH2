﻿using OpenBlam.Core.Extensions;
using OpenH2.Core.Exceptions;
using OpenH2.Core.Tags.Scenario;
using System;
using System.Diagnostics;
using Result = OpenH2.Core.Scripting.Execution.InterpreterResult;
using State = OpenH2.Core.Scripting.Execution.InterpreterState;

namespace OpenH2.Core.Scripting.Execution
{
    /*
     * TODO
     *  - ScriptInvocation
     *  - Remove allocations
     * 
     */


    /// <summary>
    /// The primary goals of this are: 1) No heap allocations to avoid GC and 2) support continuations
    /// </summary>
    public partial class ScriptIterativeInterpreter
    {
        private readonly ScenarioTag scenario;
        private readonly IScriptEngine scriptEngine;
        private readonly IScriptExecutor executor;
        private readonly Result[] variables;

        public ScriptIterativeInterpreter(
            ScenarioTag scenario,
            IScriptEngine scriptEngine,
            IScriptExecutor executor)
        {
            this.scenario = scenario;
            this.scriptEngine = scriptEngine;
            this.executor = executor;
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
                var completed = Interpret(variable.Value_H16, out var state);
                Debug.Assert(completed);

                var result = state.Result;

                // Hack to store variable data and index in same result, should probably find a better way
                if (result.Object == null)
                    result.Object = new VariableReference(i);
                else
                    result.Int = i;

                variables[i] = result;
            }
        }

        /// <summary>
        /// Entry point to setup the interpreter state/callstack with the provided node
        /// </summary>
        public void CreateState(int nodeIndex, out State state)
        {
            state = State.Create(nodeIndex);

            ResetState(ref state);
        }

        /// <summary>
        /// Used to (re)set the provided state to it's initial configuration to allow (re-)execution
        /// </summary>
        public void ResetState(ref State state)
        {
            state.Reset();

            var node = this.scenario.ScriptSyntaxNodes[state.OriginalNodeIndex];

            PushFrame(node, ref state);
        }

        private void PushFrame(ScenarioTag.ScriptSyntaxNode node, ref State state)
        {
            if (node.NodeType == NodeType.BuiltinInvocation)
            {
                PushBuiltinInvocation(node, ref state);
            }
            else if (node.NodeType == NodeType.ScriptInvocation)
            {

            }
            else
            {
                var invocation = new ScenarioTag.ScriptSyntaxNode()
                {
                    NodeType = NodeType.BuiltinInvocation,
                    DataType = node.DataType,
                    NextIndex = ushort.MaxValue,
                };

                var begin = new ScenarioTag.ScriptSyntaxNode()
                {
                    NodeType = NodeType.Expression,
                    DataType = ScriptDataType.MethodOrOperator,
                    NextIndex = (ushort)state.OriginalNodeIndex,
                };

                state.Push(new StackFrame()
                {
                    Locals = new(6),
                    OriginatingNode = invocation,
                    Current = begin,
                    Next = ushort.MaxValue
                });
            }
        }

        public bool Interpret(int nodeIndex, out State state)
        {
            CreateState(nodeIndex, out state);

            return Interpret(ref state);
        }

        /// <summary>
        /// Method that will be called repeatedly
        /// Will block until the callstack terminates or yields for continuation
        /// </summary>
        /// <returns>True if completed, false if yielded for continuation. Caller should eventually call again if return value is false</returns>
        public bool Interpret(ref State state)
        {
            state.Yield = false;
            while (state.FrameCount > 0 && state.Yield == false)
            {
                // Always defer interpretation to the invocation that created the frame, it knows what to do
                InterpretFrame(state.TopFrame.OriginatingNode, ref state);
            }

            return !state.Yield;
        }

        private void InterpretFrame(ScenarioTag.ScriptSyntaxNode node, ref State state)
        {
            switch (node.NodeType)
            {
                case NodeType.BuiltinInvocation: InterpretBuiltinFrame(ref state); break;
                case NodeType.ScriptInvocation: InterpretScriptFrame(node, ref state); break;
                default: Throw.Exception("Non-invocation node provided to InterpretInvocation"); break;
            };
        }

        private void InterpretBuiltinFrame(ref State state)
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
                InterpretBuiltinInvocation(ref state);
            }
        }

        private void InterpretScriptFrame(ScenarioTag.ScriptSyntaxNode node, ref State state)
        {
            // Need to defer to implementation, pre-gather all arguments? even support arguments for this?
        }

        private void Interpret(ScenarioTag.ScriptSyntaxNode node, ref State state)
        {
            switch (node.NodeType)
            {
                case NodeType.BuiltinInvocation: PushBuiltinInvocation(node, ref state); break;
                //case NodeType.ScriptInvocation: PushScriptInvocation(node, ref state); break;
                case NodeType.Expression: InterpretExpression(node, ref state); break;
                case NodeType.VariableAccess: InterpretVariableAccess(node, ref state); break;
                default: Throw.NotSupported($"NodeType {node.NodeType} is not supported"); break;
            };
        }

        private void PushBuiltinInvocation(ScenarioTag.ScriptSyntaxNode node, ref State state)
        {
            Debug.Assert(node.NodeType == NodeType.BuiltinInvocation);

            var invocationNode = this.scenario.ScriptSyntaxNodes[node.NodeData_H16];
            if (invocationNode.NodeType != NodeType.Expression) Throw.InterpreterException("BuiltinInvocation's first child must be an expression", state);
            if (invocationNode.DataType != ScriptDataType.MethodOrOperator) Throw.InterpreterException($"BuiltinInvocation's first child must be of type {ScriptDataType.MethodOrOperator}", state);
            if (node.OperationId != invocationNode.OperationId) Throw.InterpreterException($"BuiltinInvocation's OperationId must match the OperationId of its first child", state);

            if (state.FrameCount > 0)
            {
                // Set top frame's Current to resume at the proper node
                state.TopFrame.Current = node;
            }

            state.Push(new StackFrame()
            {
                Locals = new(6),
                OriginatingNode = node,
                Current = invocationNode,
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
            frame.Locals.Enqueue(result);
            frame.Current = node;
        }

        private void InterpretVariableAccess(ScenarioTag.ScriptSyntaxNode node, ref State state)
        {
            var result = this.variables[node.NodeData_H16];
            state.TopFrame.Locals.Enqueue(result);
            state.TopFrame.Current = node;
        }

        private void InterpretBuiltinInvocation(ref State state)
        {
            var opId = state.TopFrame.OriginatingNode.OperationId;

            // Dispatch to relevant implementation
            // Handle methods that special-case arguments
            switch (opId)
            {
                case ScriptOps.Begin: this.Begin(ref state); return;
                case ScriptOps.BeginRandom: this.BeginRandom(ref state); return;
                case ScriptOps.If: this.If(ref state); return;
                case ScriptOps.And: this.And(ref state); return;
                case ScriptOps.Or: this.Or(ref state); return;
                case ScriptOps.Set: this.Set(ref state); return;
                case ScriptOps.SleepUntil: this.SleepUntil(ref state); return;
                default: break;
            };

            // Handle methods that consume all arguments
            var argsRemain = PrepareNextArgument(ref state);
            if (argsRemain)
            {
                return;
            }

            switch (opId)
            {
                case ScriptOps.Sleep: this.Sleep(ref state); break;
                case ScriptOps.Not: this.Not(ref state); break;
                case ScriptOps.Equals: this.ValueEquals(ref state); break;
                case ScriptOps.GreaterThan: this.GreaterThan(ref state); break;
                case ScriptOps.LessThan: this.LessThan(ref state); break;
                case ScriptOps.GreaterThanOrEqual: this.GreaterThanOrEquals(ref state); break;
                case ScriptOps.LessThanOrEqual: this.LessThanOrEquals(ref state); break;

                case ScriptOps.Add: this.Add(ref state); break;
                case ScriptOps.Subtract: this.Subtract(ref state); break;
                case ScriptOps.Multiply: this.Multiply(ref state); break;
                case ScriptOps.Divide: this.Divide(ref state); break;
                case ScriptOps.Min: this.Min(ref state); break;
                case ScriptOps.Max: this.Max(ref state); break;

                default: this.DispatchMethod(ref state); break;
            }
        }

        private void Sleep(ref State state)
        {
            var frequency = state.TopFrame.Locals.Dequeue();
            state.Yield = true;
            this.executor.Delay(frequency.Short);
            CompleteFrame(ref state);
        }

        private void SleepUntil(ref State state)
        {
            var argsRemain = PrepareNextArgument(ref state);

            if (argsRemain)
            {
                return;
            }

            var args = state.TopFrame.Locals;
            var condition = args.Dequeue();
            var frequency = this.scriptEngine.TicksPerSecond;
            var timeout = -1;

            if (args.Count > 0)
                frequency = args.Dequeue().Short;

            if (args.Count > 0)
                timeout = args.Dequeue().Int;

            if (condition.Boolean)
            {
                CompleteFrame(ref state);
            }
            else
            {
                // If the condition isn't true we need to reset the current frame to re-evaluate entirely
                var sleepUntilNode = state.TopFrame.OriginatingNode;
                var deadFrame = state.Pop();
                PushFrame(sleepUntilNode, ref state);

                // We also need to yield and instruct the executor to delay by 'frequency' ticks
                state.Yield = true;
                this.executor.Delay(frequency);

                // TODO: And somehow handle the timeout
            }
        }

        private void Begin(ref State state)
        {
            var remainingExpressions = PrepareNextArgument(ref state);

            Result result = default;

            if (state.TopFrame.Locals.Count > 0)
            {
                result = state.TopFrame.Locals.Dequeue();
            }

            if (remainingExpressions == false)
            {
                if (state.TopFrame.OriginatingNode.DataType == ScriptDataType.Void)
                {
                    CompleteFrame(ref state);
                }
                else
                {
                    CompleteFrame(result, ref state);
                }
            }
        }

        // TODO: not random!
        private void BeginRandom(ref State state)
        {
            Begin(ref state);
        }

        private void If(ref State state)
        {
            // Make sure first expression is evaluated to use as condition
            if (state.TopFrame.Locals.Count == 0)
            {
                var remainingArgs = PrepareNextArgument(ref state);
                Debug.Assert(remainingArgs);
                return;
            }
            else if (state.TopFrame.Locals.Count == 1)
            {
                var condition = state.TopFrame.Locals.Peek();
                Debug.Assert(condition.DataType == ScriptDataType.Boolean);

                var remainingArgs = PrepareNextArgument(ref state);
                Debug.Assert(remainingArgs, "If requires a corresponding expression");

                if (condition.Boolean == false)
                {
                    // If the condition is false, we need to skip the 'true' expression
                    var trueExp = this.scenario.ScriptSyntaxNodes[state.TopFrame.Next];
                    Debug.Assert(trueExp.NextIndex != ushort.MaxValue, "A 'false' condition requries a corresponding expression");
                    state.TopFrame.Next = trueExp.NextIndex;
                }
            }
            else
            {
                Debug.Assert(state.TopFrame.Locals.Count == 2, "Only 'condition' and corresponding result should be stored");

                if (state.TopFrame.OriginatingNode.DataType == ScriptDataType.Void)
                {
                    CompleteFrame(ref state);
                }
                else
                {
                    var condition = state.TopFrame.Locals.Dequeue();
                    var result = state.TopFrame.Locals.Dequeue();
                    CompleteFrame(result, ref state);
                }
            }
        }

        private void Set(ref State state)
        {
            if (state.TopFrame.Locals.Count != 2)
            {
                PrepareNextArgument(ref state);
                return;
            }

            var variableRef = state.TopFrame.Locals.Dequeue();
            var value = state.TopFrame.Locals.Dequeue();

            int i;

            if (variableRef.Object is VariableReference varRef)
            {
                i = varRef.Index;
                value.Object = varRef;
            }
            else
            {
                i = variableRef.Int;
                value.Int = i;
            }

            this.variables[i] = value;
            CompleteFrame(ref state);
        }

        private void Not(ref State state)
        {
            ref var frame = ref state.TopFrame;

            Debug.Assert(frame.Locals.Count == 1);

            var operand = frame.Locals.Dequeue();
            Debug.Assert(operand.DataType == ScriptDataType.Boolean);

            CompleteFrame(Result.From(!operand.Boolean), ref state);
        }

        private void And(ref State state)
        {
            ref var frame = ref state.TopFrame;

            var argPending = PrepareNextArgument(ref state);

            if (frame.Locals.Count > 0)
            {
                var operand = frame.Locals.Dequeue();
                Debug.Assert(operand.DataType == ScriptDataType.Boolean);

                if (operand.Boolean == false)
                {
                    CompleteFrame(Result.From(false), ref state);
                    return;
                }
            }

            if (!argPending)
            {
                CompleteFrame(Result.From(true), ref state);
            }
        }

        private void Or(ref State state)
        {
            ref var frame = ref state.TopFrame;

            var argPending = PrepareNextArgument(ref state);

            if (frame.Locals.Count > 0)
            {
                var operand = frame.Locals.Dequeue();
                Debug.Assert(operand.DataType == ScriptDataType.Boolean);

                if (operand.Boolean)
                {
                    CompleteFrame(Result.From(true), ref state);
                    return;
                }
            }

            if (!argPending)
            {
                CompleteFrame(Result.From(false), ref state);
            }
        }

        private void Add(ref State state)
        {
            ref var frame = ref state.TopFrame;

            var firstOp = frame.Locals.Dequeue();

            while (frame.Locals.Count > 0)
            {
                var operand = frame.Locals.Dequeue();

                firstOp.Add(operand);
            }

            CompleteFrame(firstOp, ref state);
        }

        private void Subtract(ref State state)
        {
            ref var frame = ref state.TopFrame;

            var firstOp = frame.Locals.Dequeue();

            while (frame.Locals.Count > 0)
            {
                var operand = frame.Locals.Dequeue();

                firstOp.Subtract(operand);
            }

            CompleteFrame(firstOp, ref state);
        }

        private void Multiply(ref State state)
        {
            ref var frame = ref state.TopFrame;

            var firstOp = frame.Locals.Dequeue();

            while (frame.Locals.Count > 0)
            {
                var operand = frame.Locals.Dequeue();

                firstOp.Multiply(operand);
            }

            CompleteFrame(firstOp, ref state);
        }

        private void Divide(ref State state)
        {
            ref var frame = ref state.TopFrame;

            var firstOp = frame.Locals.Dequeue();

            while (frame.Locals.Count > 0)
            {
                var operand = frame.Locals.Dequeue();

                firstOp.Divide(operand);
            }

            CompleteFrame(firstOp, ref state);
        }

        private void Min(ref State state)
        {
            ref var frame = ref state.TopFrame;

            var firstOp = frame.Locals.Dequeue();

            while (frame.Locals.Count > 0)
            {
                var operand = frame.Locals.Dequeue();

                firstOp = Result.Min(firstOp, operand);
            }

            CompleteFrame(firstOp, ref state);
        }

        private void Max(ref State state)
        {
            ref var frame = ref state.TopFrame;

            var firstOp = frame.Locals.Dequeue();

            while (frame.Locals.Count > 0)
            {
                var operand = frame.Locals.Dequeue();

                firstOp = Result.Max(firstOp, operand);
            }

            CompleteFrame(firstOp, ref state);
        }

        private void ValueEquals(ref State state)
        {
            ref var frame = ref state.TopFrame;

            var left = frame.Locals.Dequeue();
            var right = frame.Locals.Dequeue();
            Debug.Assert(frame.Locals.Count == 0);

            var result = left.DataType switch
            {
                ScriptDataType.Boolean => Result.From(left.Boolean == right.Boolean),
                ScriptDataType.Float => Result.From(left.GetFloat() == right.GetFloat()),
                ScriptDataType.Short => Result.From(left.GetShort() == right.GetShort()),
                ScriptDataType.Int => Result.From(left.GetInt() == right.GetInt()),
                ScriptDataType.GameDifficulty => Result.From(left.GetShort() == right.GetShort()),
                _ => throw new InterpreterException($"Equality comparison for {left.DataType} is not supported")
            };

            CompleteFrame(result, ref state);
        }

        private void GreaterThan(ref State state)
        {
            ref var frame = ref state.TopFrame;

            var left = frame.Locals.Dequeue();
            var right = frame.Locals.Dequeue();
            Debug.Assert(frame.Locals.Count == 0);

            var result = left.DataType switch
            {
                ScriptDataType.Float => Result.From(left.GetFloat() > right.GetFloat()),
                ScriptDataType.Short => Result.From(left.GetShort() > right.GetShort()),
                _ => throw new InterpreterException($"Equality comparison for {left.DataType} is not supported")
            };

            CompleteFrame(result, ref state);
        }

        private void GreaterThanOrEquals(ref State state)
        {
            ref var frame = ref state.TopFrame;

            var left = frame.Locals.Dequeue();
            var right = frame.Locals.Dequeue();
            Debug.Assert(frame.Locals.Count == 0);

            var result = left.DataType switch
            {
                ScriptDataType.Float => Result.From(left.GetFloat() >= right.GetFloat()),
                ScriptDataType.Short => Result.From(left.GetShort() >= right.GetShort()),
                _ => throw new InterpreterException($"Equality comparison for {left.DataType} is not supported")
            };

            CompleteFrame(result, ref state);
        }

        private void LessThan(ref State state)
        {
            ref var frame = ref state.TopFrame;

            var left = frame.Locals.Dequeue();
            var right = frame.Locals.Dequeue();
            Debug.Assert(frame.Locals.Count == 0);

            var result = left.DataType switch
            {
                ScriptDataType.Float => Result.From(left.GetFloat() < right.GetFloat()),
                ScriptDataType.Short => Result.From(left.GetShort() < right.GetShort()),
                _ => throw new InterpreterException($"Equality comparison for {left.DataType} is not supported")
            };

            CompleteFrame(result, ref state);
        }

        private void LessThanOrEquals(ref State state)
        {
            ref var frame = ref state.TopFrame;

            var left = frame.Locals.Dequeue();
            var right = frame.Locals.Dequeue();
            Debug.Assert(frame.Locals.Count == 0);

            var result = left.DataType switch
            {
                ScriptDataType.Float => Result.From(left.GetFloat() <= right.GetFloat()),
                ScriptDataType.Short => Result.From(left.GetShort() <= right.GetShort()),
                _ => throw new InterpreterException($"Equality comparison for {left.DataType} is not supported")
            };

            CompleteFrame(result, ref state);
        }

        private bool PrepareNextArgument(ref State state)
        {
            ref var top = ref state.TopFrame;
            if (top.Current.NextIndex == ushort.MaxValue)
            {
                return false;
            }
            else
            {
                top.Next = top.Current.NextIndex;
                return true;
            }
        }

        private void CompleteFrame(InterpreterResult value, ref State state)
        {
            var completedFrame = state.Pop();

            var destType = completedFrame.OriginatingNode.DataType;

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

            if (state.FrameCount == 0)
            {
                state.Result = value;
            }
            else
            {
                state.TopFrame.Locals.Enqueue(value);
            }
        }

        private void CompleteFrame(ref State state)
        {
            var completedFrame = state.Pop();
        }

        /*
         * Source generator for each IScriptEngine method that creates an Invoke_{Method} that
         * does {number of arguments} Locals.Dequeue() to collect arguments and finally calls the 
         * IScriptEngine instance method with the arguments. Value is produced with CompleteFrame(...)
         * Then this DispatchMethodOrOperator method can be generated with a switch to each invoke, bypassing
         * the static dictionary altogether, removing array allocations for MethodInfo.Invoke and call overhead
         * 
         * Warning in IDE will show, but build will pass as the code is generated at compile-time
         */
        private partial void DispatchMethod(ref State state);
    }
}
