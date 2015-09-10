namespace Platform.Links.System.Helpers.Disposal
{
    public delegate void DisposedDelegate(bool manual);

    public class DefaultDisposal : DisposalBase
    {
        public event DisposedDelegate Disposed = m => { };

        public DefaultDisposal(DisposedDelegate disposed)
        {
            Disposed += disposed;
        }

        protected override void DisposeCore(bool manual)
        {
            Disposed(manual);
            foreach (DisposedDelegate item in Disposed.GetInvocationList()) Disposed -= item;
        }
    }
}