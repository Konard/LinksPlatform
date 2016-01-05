namespace Platform.Helpers
{
    /// <remarks>Original idea from http://geekswithblogs.net/blackrob/archive/2014/12/18/array-pooling-in-csharp.aspx </remarks>
    public class ConcurrentArrayPool<T> : ArrayPool<T>
    {
        internal override T[] Allocate(int size)
        {
            lock (this) return base.Allocate(size);
        }

        internal override void Free(T[] array)
        {
            lock (this) base.Free(array);
        }

        public override void Clear()
        {
            lock (this) base.Clear();
        }
    }
}
