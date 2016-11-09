using System.Runtime.CompilerServices;

namespace Platform.Helpers
{
    /// <remarks>
    /// Must be class, not struct (in order to persist access to Count field value).
    /// </remarks>
    public class Counter
    {
        public ulong Count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Increment() => Count++;
    }

    public class Counter<TValue> : Counter
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IncrementAndReturnTrue()
        {
            Count++;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IncrementAndReturnTrue(TValue value)
        {
            Count++;
            return true;
        }
    }

    public class Counter<TValue, TDecision> : Counter
    {
        private readonly TDecision _trueValue;

        public Counter(TDecision trueValue = default(TDecision))
        {
            _trueValue = trueValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TDecision IncrementAndReturnTrue()
        {
            Count++;
            return _trueValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TDecision IncrementAndReturnTrue(TValue value)
        {
            Count++;
            return _trueValue;
        }
    }
}
