namespace Platform.Data.Core.Pairs
{
    public interface ILinksOptions<TLink>
    {
        ILinksMemoryManager<TLink> MemoryManager { get; set; }
    }
}
