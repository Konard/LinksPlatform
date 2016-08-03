using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Platform.Helpers.Disposables;

namespace Platform.Helpers.Collections
{
    public static class ArrayPool
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T[] Allocate<T>(Integer size)
        {
            var arrayPool = ArrayPool<T>.ThreadDefault ?? (ArrayPool<T>.ThreadDefault = new ArrayPool<T>());
            return arrayPool.Allocate(size);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Free<T>(T[] array)
        {
            var arrayPool = ArrayPool<T>.ThreadDefault ?? (ArrayPool<T>.ThreadDefault = new ArrayPool<T>());
            arrayPool.Free(array);
        }
    }

    /// <remarks>
    /// Original idea from http://geekswithblogs.net/blackrob/archive/2014/12/18/array-pooling-in-csharp.aspx
    /// </remarks>
    /// <remarks>
    /// TODO: Check actual performance
    /// TODO: Check for memory leaks
    /// </remarks>
    public class ArrayPool<T>
    {
        [ThreadStatic] public static ArrayPool<T> ThreadDefault;

        private readonly Dictionary<int, Stack<T[]>> _pool = new Dictionary<int, Stack<T[]>>();

        public static readonly T[] Empty = new T[0];

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
            _pool.GetOrAdd(array.Length, size => new Stack<T[]>()).Push(array);
        }
    }
}
