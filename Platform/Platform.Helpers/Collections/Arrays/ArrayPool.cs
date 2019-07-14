using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Platform.Disposables;
using Platform.Ranges;
using Platform.Helpers.Collections.Stacks;
using Platform.Helpers.Exceptions;
using Platform.Helpers.Numbers;

namespace Platform.Helpers.Collections.Arrays
{
    /// <remarks>
    /// TODO: Check actual performance
    /// TODO: Check for memory leaks
    /// </remarks>
    public static class ArrayPool
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] Allocate<T>(Integer size) => Default.GetOrCreateThreadInstance<ArrayPool<T>>().Allocate(size);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Free<T>(T[] array) => Default.GetOrCreateThreadInstance<ArrayPool<T>>().Free(array);
    }

    /// <remarks>
    /// Original idea from http://geekswithblogs.net/blackrob/archive/2014/12/18/array-pooling-in-csharp.aspx
    /// </remarks>
    public class ArrayPool<T>
    {
        public const int DefaultSizesAmount = 512;
        public const int DefaultMaxArraysPerSize = 32;

        private readonly int _maxArraysPerSize;
        public static readonly T[] Empty = new T[0];

        private readonly Dictionary<int, Stack<T[]>> _pool = new Dictionary<int, Stack<T[]>>(DefaultSizesAmount);

        public ArrayPool(int maxArraysPerSize)
        {
            _maxArraysPerSize = maxArraysPerSize;
        }

        public ArrayPool()
           : this(DefaultMaxArraysPerSize)
        {
        }

        public Disposable<T[]> AllocateDisposable(long size) => Disposable<T[]>.Create(Allocate(size), Free);

        public Disposable<T[]> Resize(Disposable<T[]> source, long size)
        {
            var destination = AllocateDisposable(size);
            var sourceArray = source.Object;
            Array.Copy(sourceArray, destination.Object, size < sourceArray.Length ? (int)size : sourceArray.Length);
            source.Dispose();
            return destination;
        }

        public virtual void Clear() => _pool.Clear();

        public virtual T[] Allocate(long size)
        {
            Ensure.ArgumentInRange(size, new Range<long>(0, int.MaxValue));
            if (size == 0)
                return Empty;
            return _pool.GetOrDefault((int)size)?.PopOrDefault() ?? new T[size];
        }

        public virtual void Free(T[] array)
        {
            Ensure.ArgumentNotNull(array, nameof(array));
            if (array.Length == 0)
                return;
            var stack = _pool.GetOrAdd(array.Length, size => new Stack<T[]>(_maxArraysPerSize));
            if (stack.Count == _maxArraysPerSize)
                return; // Do not put the array to stack
            stack.Push(array);
        }
    }
}
