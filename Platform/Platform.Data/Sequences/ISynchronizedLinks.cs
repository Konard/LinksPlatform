using Platform.Threading.Synchronization;

namespace Platform.Data
{
    public interface ISynchronizedLinks<T> : ISynchronized<ILinks<T>>, ILinks<T>
    {
    }
}
