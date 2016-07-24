using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace Platform.Helpers.Disposables
{
    /// <summary>
    /// Представляет базовый класс реализующий основную логику необходимую для повышения вероятности корректного высвобождения памяти.
    /// </summary>
    /// <example><code source="DisposalBaseUsageExample.cs" /></example>
    /// <remarks>
    /// TODO: Попробовать реализовать компилируемый автоматический вариант DisposeCore (находить все типы IDisposable, IDisposal и автоматически вычищать их).
    /// </remarks>
    public abstract class DisposableBase : IDisposable
    {
        private static readonly Process CurrentProcess = Process.GetCurrentProcess();

        private int _disposed;

        public bool IsDisposed => _disposed > 0;

        protected virtual string ObjectName => GetType().Name;

        protected virtual bool AllowMultipleDisposeAttempts => false;

        protected virtual bool AllowMultipleDisposeCalls => false;

        protected DisposableBase()
        {
            _disposed = 0;
            CurrentProcess.Exited += OnProcessExit;
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        public void Destruct()
        {
            if (!IsDisposed)
                Dispose(false);
        }

        private void OnProcessExit(object sender, EventArgs e)
        {
            GC.SuppressFinalize(this);
            Destruct();
        }

        ~DisposableBase()
        {
            Destruct();
        }

        private void Dispose(bool manual)
        {
            var originalValue = Interlocked.CompareExchange(ref _disposed, 1, 0);

            if (AllowMultipleDisposeAttempts || originalValue == 0)
            {
                try
                {
                    if (originalValue == 0)
                    {
                        if (CurrentProcess != null)
                            CurrentProcess.Exited -= OnProcessExit;
                        //else
                        //    Process.GetCurrentProcess().Exited -= OnProcessExit;
                    }

                    DisposeCore(manual);
                }
                catch(Exception exception)
                {
                    if (!AllowMultipleDisposeAttempts || manual) throw;
                    else Global.OnIgnoredException(exception);
                }
            }
            else if (!AllowMultipleDisposeCalls && manual)
            {
                throw new ObjectDisposedException(ObjectName);
            }
        }

        protected abstract void DisposeCore(bool manual);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        protected virtual void EnsureNotDisposed()
        {
            // TODO: Make this check removable (for example enable only if Debug is enabled)
            // Faster implementation (if not disposed ObjectName is not accessed)
            if (_disposed > 0)
                throw new ObjectDisposedException(ObjectName);

            // This can reduce performance (ObjectName always accessed)
            //Ensure.NotDisposed(this, ObjectName);
        }
    }
}