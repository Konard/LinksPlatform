namespace Platform.Helpers.Threading
{
    public interface ISynchronized<TInterface>
    {
        ISynchronization SyncRoot { get; }
        TInterface Sync { get; }
        TInterface Unsync { get; }
    }
}
