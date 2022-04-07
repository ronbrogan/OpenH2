﻿using System.Threading.Tasks;

namespace OpenH2.Core.Scripting.Execution
{
    public enum ScriptExecutionMetric
    {
        MethodTiming,
    }

    public interface IScriptExecutor
    {
        void Execute();

        void SetStatus(ScriptStatus desiredStatus);
        void SetStatus(ushort methodId, ScriptStatus desiredStatus);

        /// <summary>
        /// Creates a task that will take the specified number of updates to complete
        /// </summary>
        ValueTask Delay(int ticks);

        ValueTask Delay(ushort methodId, int ticks);

        void RecordMetric(ScriptExecutionMetric metric, string dimension, long value);
    }
}
