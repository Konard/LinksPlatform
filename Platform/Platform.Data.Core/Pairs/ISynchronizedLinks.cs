using Platform.Helpers.Threading;

namespace Platform.Data.Core.Pairs
{
    public interface ISynchronizedLinks<T> : ISynchronized<ILinks<T>>, ILinks<T>
    {
    }
}
