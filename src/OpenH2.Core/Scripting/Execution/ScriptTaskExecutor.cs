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

        private ExecutionState[] executionStates = Array.Empty<ExecutionState>();

        public void Setup(ScenarioScriptBase scripts)
        {
            var scriptMethods = scripts.GetType()
                .GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(m => m.GetCustomAttribute<ScriptMethodAttribute>() != null)
                .ToArray();

            var execStates = new List<ExecutionState>(scriptMethods.Length);

            foreach (var script in scriptMethods)
            {
                var attr = script.GetCustomAttribute<ScriptMethodAttribute>();

                var initialStatus = attr.Lifecycle switch
                {
                    Lifecycle.Startup => ScriptStatus.RunOnce,
                    Lifecycle.Dormant => ScriptStatus.Sleeping,
                    Lifecycle.Continuous => ScriptStatus.RunContinuous,
                    Lifecycle.Static => ScriptStatus.Terminated,
                    Lifecycle.Stub => ScriptStatus.Terminated,
                    Lifecycle.CommandScript => ScriptStatus.Terminated,
                    _ => throw new NotImplementedException()
                };


                var execState = new ExecutionState()
                {
                    Description = script.Name,
                    Status = initialStatus,
                    Task = null,
                    SleepUntil = DateTimeOffset.MaxValue
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

                execStates.Add(execState);
            }

            this.executionStates = execStates.ToArray();
        }

        public void Execute()
        {
            for(var i = 0; i < executionStates.Length; i++)
            {
                var state = executionStates[i];

                if(state.Status == ScriptStatus.RunContinuous && (state.Task?.IsCompleted ?? true))
                {
                    state.Task = state.Func();
                }
                if (state.Status == ScriptStatus.RunOnce)
                {
                    if(state.Task == null)
                    {
                        state.Task = state.Func();
                    }
                    else if(state.Task.IsCompleted)
                    {
                        state.Status = ScriptStatus.Terminated;
                    }
                }
                if (state.Status == ScriptStatus.Sleeping && state.SleepUntil < DateTimeOffset.UtcNow)
                {
                    Logger.LogInfo($"[SCRIPT] ({state.Description}) - waking up");
                    state.Status = ScriptStatus.RunOnce;
                    state.Task = state.Func();
                }

                executionStates[i] = state;
            }
        }

        public void SetStatus(string methodName, ScriptStatus desiredStatus)
        {
            for (var i = 0; i < executionStates.Length; i++)
            {
                var state = executionStates[i];

                if (state.Description == methodName)
                {
                    if(state.Status == ScriptStatus.Terminated)
                    {
                        Logger.Log("[SCRIPT] Trying to set terminated lifecycle", Logger.Color.Red);
                        return;
                    }

                    state.Status = desiredStatus;
                    Logger.LogInfo($"[SCRIPT] ({methodName}) -> {desiredStatus}");
                }

                executionStates[i] = state;
            }
        }

        public void SleepUntil(string methodName, DateTimeOffset offset)
        {
            for (var i = 0; i < executionStates.Length; i++)
            {
                var state = executionStates[i];

                if (state.Description == methodName)
                {
                    Logger.LogInfo($"[SCRIPT] ({methodName}) @ {(offset - DateTimeOffset.UtcNow).TotalMilliseconds}");
                    state.Status = ScriptStatus.Sleeping;
                    state.SleepUntil = offset;
                }

                executionStates[i] = state;
            }
        }

        private struct ExecutionState
        {
            public string Description;
            public OrchestratedScript Func;
            public ScriptStatus Status;
            public DateTimeOffset SleepUntil;
            public Task? Task;
        }
    }
}
