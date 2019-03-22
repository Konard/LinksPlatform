namespace Platform.Data.Core.Doublets
{
    public class LinksOptions<TLink> : ILinksOptions<TLink>
    {
        public ILinksMemoryManager<TLink> MemoryManager { get; set; }
    }
}
