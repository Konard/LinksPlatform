using System;
using Platform.Interfaces;
using Platform.Memory;

namespace Platform.Data.Core.Doublets
{
    /// <remarks>
    /// TODO: Возможно нужна фабрика не для самих Links, а для всего контекста работы с ними, которое собирает всё что нужно, а также предоставляет подходящие константы.
    /// </remarks>
    internal class LinksFactory<TLink> : IFactory<ILinks<TLink>>
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
                var memoryAdapter = new UInt64ResizableDirectMemoryLinks(new HeapResizableDirectMemory(UInt64ResizableDirectMemoryLinks.DefaultLinksSizeStep)) as ILinks<TLink>;

                return new SynchronizedLinks<ulong>(new UInt64Links(memoryAdapter as ILinks<ulong>)) as ILinks<TLink>;
            }

            throw new NotImplementedException();
        }
    }
}
