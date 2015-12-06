using System;

namespace Platform.Helpers.Disposal
{
    internal class DefaultDisposalUsageExample : IDisposable
    {
        private readonly DefaultDisposal _disposal;

        public DefaultDisposalUsageExample()
        {
            _disposal = new DefaultDisposal(Disposed);
        }

        public void Dispose()
        {
            _disposal.Dispose();
        }

        ~DefaultDisposalUsageExample()
        {
            _disposal.Destruct();
        }

        private void Disposed(bool manual)
        {
            // en: Dispose logic
            // ru: Логика высвобождения памяти
        }
    }
}