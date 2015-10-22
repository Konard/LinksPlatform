namespace Platform.Helpers.Disposal
{
    public delegate void DisposedDelegate(bool manual);

    public class DefaultDisposal : DisposalBase
    {
        public event DisposedDelegate OnDispose = m => { };

        public DefaultDisposal(DisposedDelegate disposed)
        {
            OnDispose += disposed;
        }

        protected override void DisposeCore(bool manual)
        {
            OnDispose(manual);
            foreach (DisposedDelegate item in OnDispose.GetInvocationList()) OnDispose -= item;
        }
    }
}