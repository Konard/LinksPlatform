using System;

namespace Platform.Helpers.Disposal
{
    /// <remarks>
    /// TODO: Should TryDispose method catch and ignore exceptions?
    /// </remarks>
    public static class DisposalHelpers
    {
        public static bool TryDispose<T>(ref T @object)
            where T : class
        {
            var disposal = @object as DisposalBase;
            if (disposal != null)
            {
                if (!disposal.Disposed)
                {
                    disposal.Dispose();
                    @object = null;
                    return true;
                }
            }
            else
            {
                var disposable = @object as IDisposable;
                if (disposable != null)
                {
                    disposable.Dispose();
                    @object = null;
                    return true;
                }
            }

            return false;
        }

        public static bool TryDispose<T>(T @object)
            where T : class
        {
            return TryDispose(ref @object);
        }
    }
}
