using System;
using System.Runtime.InteropServices;
using Platform.Helpers;

namespace Platform.Memory
{
    /// <summary>
    /// Represents a memory block allocated in Heap.
    /// Представляет блок памяти, выделенный в "куче".
    /// </summary>
    /// <remarks>
    /// TODO: Реализовать вариант с Virtual Memory
    /// </remarks>
    public unsafe class HeapResizableDirectMemory : ResizableDirectMemoryBase
    {
        #region Logic

        public HeapResizableDirectMemory(long minimumReservedCapacity)
        {
            if (minimumReservedCapacity < MinimumCapacity)
                minimumReservedCapacity = MinimumCapacity;

            ReservedCapacity = minimumReservedCapacity;
            UsedCapacity = 0;
        }

        public HeapResizableDirectMemory()
            : this(MinimumCapacity)
        {
        }

        protected override void OnReservedCapacityChanged(long oldReservedCapacity, long newReservedCapacity)
        {
            if (Pointer == null)
            {
                Pointer = Marshal.AllocHGlobal(new IntPtr(newReservedCapacity)).ToPointer();

                MemoryHelpers.ZeroMemory(Pointer, newReservedCapacity);
            }
            else Pointer = Marshal.ReAllocHGlobal(new IntPtr(Pointer), new IntPtr(newReservedCapacity)).ToPointer();
        }

        #endregion

        #region DisposalBase

        protected override void DisposePointer(void* pointer, long size) => Marshal.FreeHGlobal(new IntPtr(pointer));

        protected override void EnsureNotDisposed() => EnsureNotDisposed("Heap stored memory block");

        #endregion
    }
}