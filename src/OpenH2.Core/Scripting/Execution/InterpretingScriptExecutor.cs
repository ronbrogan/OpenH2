using OpenH2.Core.Maps;
using OpenH2.Core.Tags.Scenario;
using OpenH2.Foundation.Logging;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace OpenH2.Core.Scripting.Execution
{
    public partial class InterpretingScriptExecutor : BaseScriptExecutor
    {
        private ExecutionState[] executionStates;
        private ScriptIterativeInterpreter interpreter;
        private int currentMethod;

        public InterpretingScriptExecutor(ScenarioTag scenario)
        {
            this.executionStates = new ExecutionState[scenario.ScriptMethods.Length];
            this.interpreter = new ScriptIterativeInterpreter(scenario, this);

            for (int i = 0; i < scenario.ScriptMethods.Length; i++)
            {
                var method = scenario.ScriptMethods[i];
                var initialStatus = method.Lifecycle switch
                {
                    Lifecycle.Startup => ScriptStatus.RunOnce,
                    Lifecycle.Dormant => ScriptStatus.Terminated,
                    Lifecycle.Continuous => ScriptStatus.RunContinuous,
                    Lifecycle.Static => ScriptStatus.Terminated,
                    Lifecycle.Stub => ScriptStatus.Terminated,
                    Lifecycle.CommandScript => ScriptStatus.Terminated,
                    _ => throw new NotImplementedException()
                };

                this.interpreter.CreateState(method.SyntaxNodeIndex, out var interpreterState);

                var state = new ExecutionState
                {
                    MethodId = i,
                    Status = initialStatus,
                    InterpreterState = interpreterState,
                    Description = method.Description
                };

                executionStates[i] = state;
            }
        }

        public void Initialize(IScriptEngine scriptEngine)
        {
            this.interpreter.Initialize(scriptEngine);
        }

        public override void Execute()
        {
            for (int i = 0; i < this.executionStates.Length; i++)
            {
                currentMethod = i;
                ref var state = ref this.executionStates[i];

                if (state.Status == ScriptStatus.RunContinuous || state.Status == ScriptStatus.RunOnce)
                {
                    var terminated = this.interpreter.Interpret(ref state.InterpreterState);

                    if(terminated)
                    {
                        interpreter.ResetState(ref state.InterpreterState);

                        if(state.Status == ScriptStatus.RunOnce)
                            state.Status = ScriptStatus.Terminated;
                    }
                }

                if (state.Status == ScriptStatus.Sleeping && --state.SleepTicksRemaining <= 0)
                {
                    state.Status = ScriptStatus.RunOnce;
                }
            }
        }

        public override ValueTask Delay(int ticks)
        {
            return Delay((ushort)currentMethod, ticks);
        }

        public override ValueTask Delay(ushort methodId, int ticks)
        {
            ref var state = ref this.executionStates[methodId];
            state.SleepTicksRemaining = ticks;
            state.Status = ScriptStatus.Sleeping;
            state.InterpreterState.Yield = true;

            return new ValueTask();
        }

        public override void SetStatus(ScriptStatus desiredStatus)
        {
            SetStatus((ushort)currentMethod, desiredStatus);
        }

        public override void SetStatus(ushort id, ScriptStatus desiredStatus)
        {
            ref var state = ref this.executionStates[id];

            state.Status = desiredStatus;
            Logger.LogInfo($"[SCRIPT] ({state.Description}) -> {desiredStatus}");
        }

        private struct ExecutionState
        {
            public int MethodId;
            public ScriptStatus Status;
            public int SleepTicksRemaining;
            public string Description;
            public InterpreterState InterpreterState;
        }
    }
}