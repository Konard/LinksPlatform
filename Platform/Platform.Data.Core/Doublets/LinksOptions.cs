namespace Platform.Data.Core.Pairs
{
    public class LinksOptions<TLink> : ILinksOptions<TLink>
    {
        public ILinksMemoryManager<TLink> MemoryManager { get; set; }
    }
}
