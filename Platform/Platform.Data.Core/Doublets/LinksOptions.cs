namespace Platform.Data.Core.Doublets
{
    internal class LinksOptions<TLink> : ILinksOptions<TLink>
    {
        public ILinksMemoryManager<TLink> MemoryManager { get; set; }
    }
}
