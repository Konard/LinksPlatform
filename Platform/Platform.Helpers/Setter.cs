using System.Runtime.CompilerServices;

namespace Platform.Helpers
{
    /// <remarks>
    /// Must be class, not struct (in order to persist access to Result field value).
    /// </remarks>
    public class Setter<T>
    {
        public T Result;

        public Setter(T defaultValue = default(T))
        {
            Result = defaultValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(T value) => Result = value;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool SetAndReturnTrue(T value)
        {
            Result = value;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool SetAndReturnFalse(T value)
        {
            Result = value;
            return false;
        }
    }
}
