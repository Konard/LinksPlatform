using System.Runtime.CompilerServices;

namespace Platform.Helpers.Setters
{
    /// <remarks>
    /// Must be class, not struct (in order to persist access to Result field value).
    /// </remarks>
    public abstract class SetterBase<TResult>
    {
        public TResult Result;

        protected SetterBase(TResult defaultValue = default) => Result = defaultValue;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(TResult value) => Result = value;
    }
}
