namespace OpenH2.Core.Metrics
{
    public interface IMetricSink
    {
        void Write(string sourceIdentifier, string plainText);
    }
}
