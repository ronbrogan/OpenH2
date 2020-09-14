using System;
using System.Threading.Tasks;

namespace OpenH2.Core.Scripting.Execution
{
    public interface IScriptExecutor
    {
        void Execute();

        void SetStatus(string methodName, ScriptStatus desiredStatus);
        void SleepUntil(string methodName, DateTimeOffset offset);
    }
}
