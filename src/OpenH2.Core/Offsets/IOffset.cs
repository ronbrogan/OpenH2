namespace OpenH2.Core.Offsets
{
    public interface IOffset
    {
        int Value { get; }
        int OriginalValue { get; }
    }
}