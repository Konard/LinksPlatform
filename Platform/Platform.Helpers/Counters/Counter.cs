using System.Runtime.CompilerServices;

namespace Platform.Helpers.Counters
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
}
