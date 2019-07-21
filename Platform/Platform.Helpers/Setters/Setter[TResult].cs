using System.Runtime.CompilerServices;

namespace Platform.Helpers.Setters
{
    public class Setter<TResult> : SetterBase<TResult>
    {
        public Setter(TResult defaultValue = default)
            : base(defaultValue)
        {
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool SetAndReturnTrue(TResult value)
        {
            Result = value;
            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool SetAndReturnFalse(TResult value)
        {
            Result = value;
            return false;
        }
    }
}
