using OpenH2.Core.Metrics;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenH2.Core.Scripting.Execution
{
    public abstract class BaseScriptExecutor : IScriptExecutor, IMetricSource
    {
        private bool metricsEnabled = false;
        private IMetricSink metricSink;

        public abstract ValueTask Delay(int ticks);
        public abstract ValueTask Delay(ushort methodId, int ticks);
        public abstract void Execute();
        public abstract void SetStatus(ScriptStatus desiredStatus);
        public abstract void SetStatus(ushort methodId, ScriptStatus desiredStatus);


        public void RecordMetric(ScriptExecutionMetric metric, string dimension, long value)
        {
            if (!metricsEnabled) return;

            metricSink.Write(nameof(IScriptExecutor), $"{metric},{dimension},{value}");
        }

        public void Enable(IMetricSink destination)
        {
            metricSink = destination;
            metricsEnabled = true;
        }
    }
}
