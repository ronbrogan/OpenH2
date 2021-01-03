using System;
using System.Threading.Tasks;

namespace OpenH2.Core.Scripting.Execution
{
    public interface IScriptExecutor
    {
        void Execute();

        void SetStatus(ScriptStatus desiredStatus);
        void SetStatus(ushort methodId, ScriptStatus desiredStatus);

        /// <summary>
        /// Creates a task that will take the specified number of updates to complete
        /// </summary>
        ValueTask Delay(int ticks);
    }
}
