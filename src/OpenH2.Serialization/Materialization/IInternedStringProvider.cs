namespace OpenH2.Serialization.Materialization
{
    public interface IInternedStringProvider
    {
        int IndexOffset { get; }
        int DataOffset { get; }
    }
}
