using System;
using Platform.WindowsAPI;

namespace Platform.Memory
{
    /// <summary>
    /// Represents a memory block allocated inside Windows Heap.
    /// </summary>
    /// <remarks>
    /// TODO: Реализовать вариант с Virtual Memory
    /// TODO: После использования WinApi подумать над реализацией под Mono
    /// </remarks>
    public unsafe class HeapResizableDirectMemory : ResizableDirectMemoryBase
    {
        #region Logic

        public HeapResizableDirectMemory(long minimumReservedCapacity)
        {
            if (minimumReservedCapacity < 0)
                throw new ArgumentOutOfRangeException("minimumReservedCapacity");

            ReservedCapacity = minimumReservedCapacity;
            UsedCapacity = 0;
        }

        public HeapResizableDirectMemory()
            : this(0)
        {
        }

        protected override void OnReservedCapacityChanged(long oldReservedCapacity, long newReservedCapacity)
        {
            Pointer = Pointer == null ? Kernel32.HeapAlloc(newReservedCapacity) : Kernel32.HeapReAlloc(Pointer, newReservedCapacity);
        }

        #endregion

        #region DisposalBase

        protected override void DisposePointer(void* pointer, long size)
        {
            Kernel32.HeapFree(pointer);
        }

        protected override void EnsureNotDisposed()
        {
            EnsureNotDisposed("Heap stored memory block");
        }

        #endregion
    }
}