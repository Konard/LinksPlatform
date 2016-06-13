using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Platform.Helpers
{
    /// <remarks>
    /// Looks like a serious optimizations needed here.
    /// </remarks>
    public static class IntPtrExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TElement GetValue<TElement>(this IntPtr pointer)
        {
#if NET45
            return (TElement)Marshal.PtrToStructure(pointer, typeof(TElement));
#else
            return Marshal.PtrToStructure<TElement>(pointer);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetValue<TElement>(this IntPtr pointer, TElement value)
        {
            Marshal.StructureToPtr(value, pointer, true);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IntPtr GetElement(this IntPtr pointer, int elementSize, int index)
        {
            return pointer + elementSize * index;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe IntPtr GetElement(this IntPtr pointer, long elementSize, long index)
        {
            return new IntPtr((byte*)pointer.ToPointer() + elementSize * index);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IntPtr GetElement<TIndex>(this IntPtr pointer, int elementSize, TIndex index)
        {
            return pointer.GetElement((long)elementSize, (Integer)(Integer<TIndex>)index);
        }
    }
}
