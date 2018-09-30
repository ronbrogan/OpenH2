namespace OpenH2.Core.Offsets
{
    internal interface IOffset
    {
        int Value { get; }
        int OriginalValue { get; }
    }
}