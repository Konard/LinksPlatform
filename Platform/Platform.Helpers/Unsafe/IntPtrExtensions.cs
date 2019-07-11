using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Platform.Helpers.Reflection;
using System.Reflection;

namespace Platform.Helpers.Unsafe
{
    public static class IntPtrExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TElement GetValue<TElement>(this IntPtr pointer)
        {
            return IntPtrExtensions<TElement>.CompiledGetValue(pointer);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetValue<TElement>(this IntPtr pointer, TElement value)
        {
            IntPtrExtensions<TElement>.CompiledSetValue(pointer, value);
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

    public static class IntPtrExtensions<T>
    {
        public static readonly Func<IntPtr, T> CompiledGetValue;
        public static readonly Action<IntPtr, T> CompiledSetValue;

        static IntPtrExtensions()
        {
            DelegateHelpers.Compile(out CompiledGetValue, emiter =>
            {
                if (CachedTypeInfo<T>.IsNumeric)
                {
                    emiter.LoadArgument(0);
                    emiter.LoadIndirect<T>();
                    emiter.Return();
                }
                else
                {
                    emiter.LoadArguments(0);
                    emiter.Call(typeof(Marshal).GetGenericMethod("PtrToStructure", new[] { typeof(T) }, new[] { typeof(IntPtr), typeof(Type), typeof(bool) }));
                    emiter.Return();
                }
            });

            DelegateHelpers.Compile(out CompiledSetValue, emiter =>
            {
                if (CachedTypeInfo<T>.IsNumeric)
                {
                    emiter.LoadArguments(0, 1);
                    emiter.StoreIndirect<T>();
                    emiter.Return();
                }
                else
                {
                    emiter.LoadArguments(0, 1);
                    emiter.LoadConstant(true);
                    emiter.Call(typeof(Marshal).GetTypeInfo().GetMethod("StructureToPtr", new[] { typeof(object), typeof(IntPtr), typeof(bool) }));
                    emiter.Return();
                }
            });
        }
    }
}
