using Platform.Threading.Synchronization;
using Platform.Data.Constants;

namespace Platform.Data
{
    // TODO: Move to Platform.Data
    public interface ISynchronizedLinks<TLink, TLinks, TConstants> : ISynchronized<TLinks>, ILinks<TLink, TConstants>
        where TConstants : ILinksCombinedConstants<TLink, TLink, int, TConstants>
        where TLinks : ILinks<TLink, TConstants>
    {
    }
}
