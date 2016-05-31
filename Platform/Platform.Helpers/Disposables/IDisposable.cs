namespace Platform.Helpers.Disposables
{
    public interface IDisposable : System.IDisposable
    {
        bool IsDisposed { get; }
        void Destruct();
    }
}