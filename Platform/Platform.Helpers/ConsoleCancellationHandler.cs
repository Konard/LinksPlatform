using System;
using System.Threading;
using Platform.Helpers.Disposables;
using Platform.Helpers.Threading;

namespace Platform.Helpers
{
    public class ConsoleCancellationHandler : DisposableBase
    {
        public CancellationTokenSource Source { get; private set; }
        public CancellationToken Token { get; private set; }

        public bool IsCancellationRequested { get => Source.IsCancellationRequested; }

        public bool NoCancellationRequested { get => !Source.IsCancellationRequested; }

        public ConsoleCancellationHandler(bool showDefaultIntroMessage = true)
        {
            if (showDefaultIntroMessage)
                Console.WriteLine("Press CTRL+C to stop.");

            Source = new CancellationTokenSource();
            Token = Source.Token;

            Console.CancelKeyPress += OnCancelKeyPress;
        }

        public void ForceCancellation() => Source.Cancel();

        private void OnCancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            e.Cancel = true;

            if (!Source.IsCancellationRequested)
            {
                Source.Cancel();
                Console.WriteLine("Stopping...");
            }
        }

        public void Wait()
        {
            while (NoCancellationRequested) ThreadHelpers.Sleep();
        }

        protected override void DisposeCore(bool manual)
        {
            if (manual)
            {
                if (!IsDisposed)
                    Console.CancelKeyPress -= OnCancelKeyPress;
                Disposable.TryDispose(Source);
            }
        }
    }
}
