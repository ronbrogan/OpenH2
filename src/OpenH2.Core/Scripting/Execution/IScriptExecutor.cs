using System;
using System.Threading.Tasks;

namespace OpenH2.Core.Scripting.Execution
{
    public interface IScriptExecutor
    {
        void Startup();
        void Execute();

        void SetLifecycle(string methodName, Lifecycle desiredLifecycle);
        void SleepUntil(string methodName, DateTimeOffset offset);
    }
}
