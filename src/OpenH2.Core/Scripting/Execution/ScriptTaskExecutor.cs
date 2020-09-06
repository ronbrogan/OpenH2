using OpenH2.Foundation.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace OpenH2.Core.Scripting.Execution
{
    public class ScriptTaskExecutor : IScriptExecutor
    {
        private delegate Task OrchestratedScript();

        private List<ExecutionState> executionStates;

        public void Setup(ScenarioScriptBase scripts)
        {
            var scriptMethods = scripts.GetType()
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.GetCustomAttribute<ScriptMethodAttribute>() != null)
                .ToArray();

            executionStates = new List<ExecutionState>(scriptMethods.Length);

            foreach (var script in scriptMethods)
            {
                var attr = script.GetCustomAttribute<ScriptMethodAttribute>();
                var execState = new ExecutionState()
                {
                    Description = script.Name,
                    Lifecycle = attr.Lifecycle,
                    Task = Task.CompletedTask,
                    DormantUntil = DateTimeOffset.MaxValue
                };

                OrchestratedScript func;

                // Top level scripts shouldn't have a return value, but we'll wrap the func
                // in an expression to allow us to invoke it if necessary
                if(script.ReturnType.IsGenericType && script.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
                {
                    func = Expression.Lambda<OrchestratedScript>(
                        Expression.Convert(
                            Expression.Call(Expression.Constant(scripts), script),
                            typeof(Task))).Compile();
                }
                else
                {
                    func = (OrchestratedScript)script.CreateDelegate(typeof(OrchestratedScript), scripts);
                }

                execState.Func = func;

                executionStates.Add(execState);
            }
        }

        public void Startup()
        {
            for (var i = 0; i < executionStates.Count; i++)
            {
                var state = executionStates[i];

                if (state.Lifecycle == Lifecycle.Startup && state.Task.IsCompleted)
                {
                    Logger.LogInfo($"[SCRIPT] ({state.Description}) - startup");
                    state.Task = state.Func();
                }

                executionStates[i] = state;
            }

            this.Execute();
        }

        public void Execute()
        {
            for(var i = 0; i < executionStates.Count; i++)
            {
                var state = executionStates[i];

                if(state.Lifecycle == Lifecycle.Continuous && state.Task.IsCompleted)
                {
                    state.Task = state.Func();
                }
                if (state.Lifecycle == Lifecycle.Dormant && state.DormantUntil < DateTimeOffset.UtcNow)
                {
                    Logger.LogInfo($"[SCRIPT] ({state.Description}) - waking up");
                    state.Lifecycle = Lifecycle.Continuous;
                    state.Task = state.Func();
                }

                executionStates[i] = state;
            }
        }

        public void SetLifecycle(string methodName, Lifecycle desiredLifecycle)
        {
            for (var i = 0; i < executionStates.Count; i++)
            {
                var state = executionStates[i];

                if (state.Description == methodName)
                {
                    state.Lifecycle = desiredLifecycle;
                    Logger.LogInfo($"[SCRIPT] ({methodName}) -> {desiredLifecycle}");
                }

                executionStates[i] = state;
            }
        }

        public void SleepUntil(string methodName, DateTimeOffset offset)
        {
            for (var i = 0; i < executionStates.Count; i++)
            {
                var state = executionStates[i];

                if (state.Description == methodName)
                {
                    Logger.LogInfo($"[SCRIPT] ({methodName}) @ {(offset - DateTimeOffset.UtcNow).TotalMilliseconds}");
                    state.Lifecycle = Lifecycle.Dormant;
                    state.DormantUntil = offset;
                }

                executionStates[i] = state;
            }
        }

        private struct ExecutionState
        {
            public string Description;
            public OrchestratedScript Func;
            public Lifecycle Lifecycle;
            public DateTimeOffset DormantUntil;
            public Task Task;
        }
    }
}
