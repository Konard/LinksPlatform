using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Platform.Disposables;
using Platform.Ranges;
using Platform.Exceptions;
using Platform.Helpers.Collections.Stacks;

namespace Platform.Helpers.Collections.Arrays
{
    /// <remarks>
    /// TODO: Check actual performance
    /// TODO: Check for memory leaks
    /// </remarks>
    public static class ArrayPool
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] Allocate<T>(long size) => ArrayPool<T>.GetOrCreateThreadInstance().Allocate(size);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Free<T>(T[] array) => ArrayPool<T>.GetOrCreateThreadInstance().Free(array);
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

        // May be use Default class for that later.
        [ThreadStatic]
        internal static ArrayPool<T> ThreadInstance;
        internal static ArrayPool<T> GetOrCreateThreadInstance() => ThreadInstance ?? (ThreadInstance = new ArrayPool<T>());

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
