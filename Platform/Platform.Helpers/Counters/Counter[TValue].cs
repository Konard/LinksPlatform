using System.Runtime.CompilerServices;

namespace Platform.Helpers.Counters
{
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
}
