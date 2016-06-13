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
            if (Pointer == IntPtr.Zero)
            {
                Pointer = Marshal.AllocHGlobal(new IntPtr(newReservedCapacity));

                MemoryHelpers.ZeroMemory(Pointer.ToPointer(), newReservedCapacity);
            }
            else Pointer = Marshal.ReAllocHGlobal(Pointer, new IntPtr(newReservedCapacity));
        }

        #endregion

        #region DisposalBase

        protected override void DisposePointer(IntPtr pointer, long size) => Marshal.FreeHGlobal(pointer);

        protected override string ObjectName => "Heap stored memory block";

        #endregion
    }
}