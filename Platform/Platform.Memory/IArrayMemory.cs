namespace Platform.Memory
{
    public interface IArrayMemory<TElement> : IMemory
    {
        TElement this[long index] { get; set; }
    }
}
