using Platform.Disposables;

namespace Platform.Examples
{
    internal class DisposableBaseUsageExample : DisposableBase
    {
        protected override void DisposeCore(bool manual, bool wasDisposed)
        {
            // en: Dispose logic
            // ru: Логика высвобождения памяти
        }
    }
}