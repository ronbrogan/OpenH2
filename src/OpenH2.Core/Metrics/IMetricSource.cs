namespace OpenH2.Core.Metrics
{
    public interface IMetricSource
    {
        void Enable(IMetricSink destination);
    }
}
