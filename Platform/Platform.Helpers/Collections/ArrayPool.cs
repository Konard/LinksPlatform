using System;
using System.Collections.Concurrent;
using Platform.Helpers.Disposables;

namespace Platform.Helpers.Collections
{
    public static class ArrayPool
    {
        public static T[] Allocate<T>(Integer size)
        {
            return Default<ArrayPool<T>>.Instance.Allocate(size);
        }

        public static void Free<T>(T[] array)
        {
            Default<ArrayPool<T>>.Instance.Free(array);
        }
    }

    /// <remarks>
    /// Original idea from http://geekswithblogs.net/blackrob/archive/2014/12/18/array-pooling-in-csharp.aspx
    /// </remarks>
    public class ArrayPool<T>
    {
        private readonly ConcurrentDictionary<int, ConcurrentStack<T[]>> _pool = new ConcurrentDictionary<int, ConcurrentStack<T[]>>();

        public static readonly T[] Empty = new T[0];

        public Disposable<T[]> AllocateDisposable(long size)
        {
            return Disposable<T[]>.Create(Allocate(size), Free);
        }

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
            _pool.GetOrAdd(array.Length, size => new ConcurrentStack<T[]>()).Push(array);
        }
    }
}
