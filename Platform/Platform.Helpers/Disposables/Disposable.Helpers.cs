using System;

namespace Platform.Helpers.Disposables
{
    /// <summary>
    /// It was DisposableHelpers, but as I actually have Disposable, it can be a direct static extension.
    /// </summary>
    public partial class Disposable
    {
        public static bool TryDispose<T>(ref T @object)
        {
            try
            {
                var disposal = @object as DisposableBase;
                if (disposal != null)
                {
                    if (!disposal.IsDisposed)
                    {
                        disposal.Dispose();
                        @object = default(T);
                        return true;
                    }
                }
                else
                {
                    var disposable = @object as System.IDisposable;
                    if (disposable != null)
                    {
                        disposable.Dispose();
                        @object = default(T);
                        return true;
                    }
                }

            }
            catch (Exception exception)
            {
                Global.OnIgnoredException(exception);
            }

            return false;
        }

        public static bool TryDispose<T>(T @object)
        {
            return TryDispose(ref @object);
        }
    }

    public partial class Disposable<T>
    {
        public static Disposable<T> Create(T value, Action<T> dispose)
        {
            return new Disposable<T>(value, dispose);
        }
    }
}