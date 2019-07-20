using System;
using System.Runtime.CompilerServices;
using Platform.Numbers;

namespace Platform.Helpers.Unsafe
{
    public static class IntPtrExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TElement GetValue<TElement>(this IntPtr pointer) => IntPtrHelpers<TElement>.GetValue(pointer);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetValue<TElement>(this IntPtr pointer, TElement value) => IntPtrHelpers<TElement>.SetValue(pointer, value);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IntPtr GetElement(this IntPtr pointer, int elementSize, int index) => pointer + elementSize * index;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe IntPtr GetElement(this IntPtr pointer, long elementSize, long index) => new IntPtr((byte*)pointer.ToPointer() + elementSize * index);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IntPtr GetElement<TIndex>(this IntPtr pointer, int elementSize, TIndex index) => pointer.GetElement((long)elementSize, (Integer)(Integer<TIndex>)index);
    }
}
