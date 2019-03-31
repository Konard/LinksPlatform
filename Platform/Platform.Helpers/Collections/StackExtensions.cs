using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Platform.Helpers.Collections
{
    public static class StackExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T PopOrDefault<T>(this Stack<T> stack) => stack.Count > 0 ? stack.Pop() : default(T);
    }
}
