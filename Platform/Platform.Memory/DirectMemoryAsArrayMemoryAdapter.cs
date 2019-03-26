using System;
using Platform.Helpers;

namespace Platform.Memory
{
    public class DirectMemoryAsArrayMemoryAdapter<TElement> : IArrayMemory<TElement>, IDirectMemory
        where TElement : struct
    {
        private readonly IDirectMemory _memory;
        private readonly int _elementSize;

        public long Size => _memory.Size;

        public IntPtr Pointer => _memory.Pointer;

        public TElement this[long index]
        {
            get
            {
                return Pointer.GetElement((long)_elementSize, index).GetValue<TElement>();
            }
            set
            {
                Pointer.GetElement((long)_elementSize, index).SetValue(value);
            }
        }

        public DirectMemoryAsArrayMemoryAdapter(IDirectMemory memory)
        {
            _elementSize = UnsafeHelpers.SizeOf<TElement>();

            _memory = memory;

            if (_memory.Size % _elementSize > 0)
                throw new ArgumentException("Memory is not aligned to element size.", nameof(memory));
        }

        public void Dispose()
        {
            _memory.Dispose();
        }
    }
}
