using System;
using System.Runtime.InteropServices;

namespace Platform.Memory
{
    /// <summary>
    /// Represents a memory block allocated inside Windows Heap.
    /// </summary>
    /// <remarks>
    /// TODO: Реализовать вариант с Virtual Memory
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
            Pointer = Pointer == null
                ? Marshal.AllocHGlobal(new IntPtr(newReservedCapacity)).ToPointer()
                : Marshal.ReAllocHGlobal(new IntPtr(Pointer), new IntPtr(newReservedCapacity)).ToPointer();
        }

        #endregion

        #region DisposalBase

        protected override void DisposePointer(void* pointer, long size)
        {
            Marshal.FreeHGlobal(new IntPtr(pointer));
        }

        protected override void EnsureNotDisposed()
        {
            EnsureNotDisposed("Heap stored memory block");
        }

        #endregion
    }
}