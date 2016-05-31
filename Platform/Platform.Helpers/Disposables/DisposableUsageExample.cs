namespace Platform.Helpers.Disposables
{
    /// <summary>
    /// TODO: Move to Platform.Helpers.Examples or Platform.Examples
    /// </summary>
    internal class DisposableUsageExample : System.IDisposable
    {
        private readonly Disposable _disposable;

        public DisposableUsageExample()
        {
            _disposable = new Disposable(Disposed);
        }

        public void Dispose() => _disposable.Dispose();

        ~DisposableUsageExample()
        {
            _disposable.Destruct();
        }

        private void Disposed(bool manual)
        {
            // en: Dispose logic
            // ru: Логика высвобождения памяти
        }
    }
}