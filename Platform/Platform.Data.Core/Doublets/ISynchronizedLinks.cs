using Platform.Helpers.Threading;

namespace Platform.Data.Core.Doublets
{
    public interface ISynchronizedLinks<T> : ISynchronized<ILinks<T>>, ILinks<T>
    {
    }
}
