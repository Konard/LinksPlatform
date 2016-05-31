using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace Platform.Helpers.Collections
{
    public static class ConcurrentDictionaryExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TValue GetOrDefault<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dictionary, TKey key)
        {
            TValue value;
            return !dictionary.TryGetValue(key, out value) ? default(TValue) : value;
        }
    }
}
