namespace Platform.Data.Core.Doublets
{
    public interface ILinksOptions<TLink>
    {
        ILinksMemoryManager<TLink> MemoryManager { get; set; }
    }
}
