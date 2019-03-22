using Platform.Helpers;

namespace Platform.Data.Core.Doublets
{
    public abstract class LinksBase<TAddress, TDecision, TPartIndex>
    {
        public readonly ILinksMemoryManager<TAddress> Memory;

        protected LinksBase(ILinksMemoryManager<TAddress> memory, ILinksCombinedConstants<TDecision, TAddress, TPartIndex> constants)
        {
            Memory = memory;
            Constants = constants;
        }

        protected LinksBase(ILinksMemoryManager<TAddress> memory) : this(memory, Use<ILinksCombinedConstants<TDecision, TAddress, TPartIndex>>.Single) {}

        public ILinksCombinedConstants<TDecision, TAddress, TPartIndex> Constants { get; }
    }
}