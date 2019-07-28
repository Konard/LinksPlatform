using Platform.Threading.Synchronization;

namespace Platform.Data.Sequences
{
    public interface ISynchronizedLinks<T> : ISynchronized<ILinks<T>>, ILinks<T>
    {
    }
}
