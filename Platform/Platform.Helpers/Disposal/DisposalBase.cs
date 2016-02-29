using System;
using System.Diagnostics;
using System.Threading;

namespace Platform.Helpers.Disposal
{
    /// <summary>
    /// Представляет базовый класс реализующий основную логику необходимую для повышения вероятности корректного высвобождения памяти.
    /// </summary>
    /// <example><code source="DisposalBaseUsageExample.cs" /></example>
    /// <remarks>
    /// TODO: Попробовать реализовать компилируемый автоматический вариант DisposeCore (находить все типы IDisposable, IDisposal и автоматически вычищать их).
    /// </remarks>
    public abstract class DisposalBase : IDisposal
    {
        private static readonly Process CurrentProcess = Process.GetCurrentProcess();

        private int _disposed;

        public bool Disposed { get { return _disposed > 0; } }

        protected virtual string ObjectName
        {
            get
            {
                return GetType().Name;
            }
        }

        protected virtual bool AllowMultipleDisposeAttempts
        {
            get
            {
                return false;
            }
        }

        protected virtual bool AllowMultipleDisposeCalls
        {
            get
            {
                return false;
            }
        }

        protected DisposalBase()
        {
            _disposed = 0;
            CurrentProcess.Exited += OnProcessExit;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Destruct()
        {
            Dispose(false);
        }

        private void OnProcessExit(object sender, EventArgs e)
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~DisposalBase()
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
                catch
                {
                    if (!AllowMultipleDisposeAttempts && manual) throw;
                    // else TODO: Log exception
                }
            }
            else if (!AllowMultipleDisposeCalls && manual)
            {
                throw new ObjectDisposedException(ObjectName);
            }
        }

        protected abstract void DisposeCore(bool manual);

        protected void EnsureNotDisposed(string objectName)
        {
            if (_disposed > 0) throw new ObjectDisposedException(objectName);
        }

        protected virtual void EnsureNotDisposed()
        {
            EnsureNotDisposed(ObjectName);
        }
    }
}