using System.Runtime.CompilerServices;

namespace Platform.Helpers
{
    public class Counter
    {
        public ulong Count;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Increment()
        {
            Count++;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IncrementAndReturnTrue()
        {
            Count++;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IncrementAndReturnTrue(ulong value)
        {
            Count++;
            return true;
        }
    }
}
