using System;

namespace Platform.Helpers.Disposal
{
    /// <remarks>Original idea from http://geekswithblogs.net/blackrob/archive/2014/12/18/array-pooling-in-csharp.aspx </remarks>
    public class DisposableValue<T> : DisposalBase
    {
        private readonly Action<T> _dispose;
        public readonly T Value;

        public DisposableValue(T value, Action<T> dispose)
        {
            _dispose = dispose;
            Value = value;
        }

        public static DisposableValue<T> Create(T value, Action<T> dispose)
        {
            return new DisposableValue<T>(value, dispose);
        }

        protected override void DisposeCore(bool manual)
        {
            _dispose(Value);
        }
    }
}
