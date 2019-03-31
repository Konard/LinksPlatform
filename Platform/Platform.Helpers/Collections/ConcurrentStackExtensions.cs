using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace Platform.Helpers.Collections
{
    public static class ConcurrentStackExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T PopOrDefault<T>(this ConcurrentStack<T> stack)
        {
            T value;
            return stack.TryPop(out value) ? value :  default(T);
        }
    }
}
