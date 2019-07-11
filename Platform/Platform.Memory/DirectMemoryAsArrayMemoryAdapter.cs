using System;
using Platform.Helpers.Disposables;
using Platform.Helpers.Unsafe;

namespace Platform.Memory
{
    public class DirectMemoryAsArrayMemoryAdapter<TElement> : DisposableBase, IArrayMemory<TElement>, IDirectMemory
        where TElement : struct
    {
        public static readonly long ElementSize = UnsafeHelpers.SizeOf<TElement>();

        private readonly IDirectMemory _memory;

        public long Size => _memory.Size;

        public IntPtr Pointer => _memory.Pointer;

        public TElement this[long index]
        {
            get => Pointer.GetElement(ElementSize, index).GetValue<TElement>();
            set => Pointer.GetElement(ElementSize, index).SetValue(value);
        }

        public DirectMemoryAsArrayMemoryAdapter(IDirectMemory memory)
        {
            _memory = memory;

            if (_memory.Size % ElementSize > 0)
                throw new ArgumentException("Memory is not aligned to element size.", nameof(memory));
        }

        protected override void DisposeCore(bool manual, bool wasDisposed)
        {
            if (!wasDisposed)
                _memory.Dispose();
        }
    }
}
