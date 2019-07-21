using System.Runtime.CompilerServices;

namespace Platform.Helpers.Counters
{
    public class Counter<TValue, TDecision> : Counter
    {
        private readonly TDecision _trueValue;

        public Counter(TDecision trueValue = default) => _trueValue = trueValue;

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
