using System;
using Platform.Helpers;
using Platform.Memory;

namespace Platform.Data.Core.Pairs
{
    /// <remarks>
    /// TODO: Возможно нужна фабрика не для самих Links, а для всего контекста работы с ними, которое собирает всё что нужно, а также предоставляет подходящие константы.
    /// </remarks>
    public class LinksFactory<TLink> : IFactory<ILinks<TLink>>
    {
        private LinksOptions<TLink> _options;

        public LinksFactory(LinksOptions<TLink> options)
        {
            _options = options;
        }

        public ILinks<TLink> Create()
        {
            if (_options == null)
                _options = new LinksOptions<TLink>();

            if (typeof(TLink) == typeof(ulong))
            {
                if (_options.MemoryManager == null)
                    _options.MemoryManager = new UInt64LinksMemoryManager(new HeapResizableDirectMemory(UInt64LinksMemoryManager.DefaultLinksSizeStep)) as ILinksMemoryManager<TLink>;

                return new SynchronizedLinks<ulong>(new UInt64Links(_options as ILinksOptions<ulong>)) as ILinks<TLink>;
            }

            throw new NotImplementedException();
        }
    }
}
