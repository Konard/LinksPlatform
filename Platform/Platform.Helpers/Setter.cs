using System.Runtime.CompilerServices;

namespace Platform.Helpers
{
    /// <remarks>
    /// Must be class, not struct (in order to persist access to Result field value).
    /// </remarks>
    public abstract class SetterBase<TResult>
    {
        public TResult Result;

        protected SetterBase(TResult defaultValue = default(TResult))
        {
            Result = defaultValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(TResult value) => Result = value;
    }

    public class Setter<TResult> : SetterBase<TResult>
    {
        public Setter(TResult defaultValue = default(TResult))
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

    public class Setter<TResult, TDecision> : SetterBase<TResult>
    {
        private readonly TDecision _trueValue;
        private readonly TDecision _falseValue;

        public Setter(TDecision trueValue = default(TDecision), TDecision falseValue = default(TDecision), TResult defaultValue = default(TResult))
            : base(defaultValue)
        {
            _trueValue = trueValue;
            _falseValue = falseValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TDecision SetAndReturnTrue(TResult value)
        {
            Result = value;
            return _trueValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TDecision SetAndReturnFalse(TResult value)
        {
            Result = value;
            return _falseValue;
        }
    }
}
