using OpenH2.Core.Maps;
using OpenH2.Core.Tags.Scenario;
using System;
using System.Threading.Tasks;

namespace OpenH2.Core.Scripting.Execution
{
    public partial class InterpretingScriptExecutor : IScriptExecutor
    {
        private readonly IH2Map map;
        private readonly ScenarioTag scenario;
        private readonly IScriptEngine scriptEngine;
        private ExecutionState[] executionStates;
        private ScriptInterpreter interpreter;

        public InterpretingScriptExecutor(IH2Map map, ScenarioTag scenario, IScriptEngine scriptEngine)
        {
            this.map = map;
            this.scenario = scenario;
            this.scriptEngine = scriptEngine;
            this.executionStates = new ExecutionState[scenario.ScriptMethods.Length];
            this.interpreter = new ScriptInterpreter(scenario, scriptEngine);

            for (int i = 0; i < scenario.ScriptMethods.Length; i++)
            {
                var method = scenario.ScriptMethods[i];
                var initialStatus = method.Lifecycle switch
                {
                    Lifecycle.Startup => ScriptStatus.RunOnce,
                    Lifecycle.Dormant => ScriptStatus.Sleeping,
                    Lifecycle.Continuous => ScriptStatus.RunContinuous,
                    Lifecycle.Static => ScriptStatus.Terminated,
                    Lifecycle.Stub => ScriptStatus.Terminated,
                    Lifecycle.CommandScript => ScriptStatus.Terminated,
                    _ => throw new NotImplementedException()
                };

                var state = new ExecutionState
                {
                    MethodId = i,
                    Status = initialStatus,
                    CurrentInstruction = method.SyntaxNodeIndex,
                };

                executionStates[i] = state;
            }
        }

        public void Execute()
        {
            for (int i = 0; i < this.executionStates.Length; i++)
            {
                ref var state = ref this.executionStates[i];

                if (state.Status == ScriptStatus.RunContinuous || state.Status == ScriptStatus.RunOnce)
                {
                    Run(ref state);
                }
            }
        }

        public Task Delay(int ticks)
        {
            return Task.CompletedTask;
        }

        public void SetStatus(ScriptStatus desiredStatus)
        {
        }

        public void SetStatus(string methodName, ScriptStatus desiredStatus)
        {
        }

        public void SleepUntil(string methodName, DateTimeOffset offset)
        {
        }

        private void Run(ref ExecutionState state)
        {
            while (state.CurrentInstruction != ushort.MaxValue)
            {
                var node = this.scenario.ScriptSyntaxNodes[state.CurrentInstruction];

                // Top level methods shouldn't return any value, discarding result
                _ = this.interpreter.Interpret(node, out state.CurrentInstruction);
            }
        }

        private struct ExecutionState
        {
            public int MethodId;
            public ushort CurrentInstruction;
            public ScriptStatus Status;
        }
    }
}