using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Platform.Helpers.Setters
{
    public class Setter<TResult, TDecision> : SetterBase<TResult>
    {
        private readonly TDecision _trueValue;
        private readonly TDecision _falseValue;

        public Setter(TDecision trueValue = default, TDecision falseValue = default, TResult defaultValue = default)
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TDecision SetFirstAndReturnTrue(IList<TResult> list)
        {
            Result = list[0];
            return _trueValue;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public TDecision SetFirstAndReturnFalse(IList<TResult> list)
        {
            Result = list[0];
            return _falseValue;
        }
    }
}
