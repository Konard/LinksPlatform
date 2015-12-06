namespace Platform.Helpers.Disposal
{
    public delegate void DisposedDelegate(bool manual);

    /// <example><code source="DefaultDisposalUsageExample.cs" /></example>
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