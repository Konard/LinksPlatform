using System;
using Sigil;

namespace Platform.Helpers.Reflection
{
    public static class DelegateHelpers
    {
        public static void Compile<TDelegate>(out TDelegate @delegate, Action<Emit<TDelegate>> emitCode)
        {
            @delegate = default;

            try
            {
                var emiter = Emit<TDelegate>.NewDynamicMethod();
                emitCode(emiter);
                @delegate = emiter.CreateDelegate();
            }
            catch (Exception exception)
            {
                Global.OnIgnoredException(exception);
            }
            finally
            {
                if (Equals(@delegate, default(TDelegate)))
                    @delegate = Singleton.Get(Default<NotSupportedExceptionDelegateFactory<TDelegate>>.Instance);
            }
        }
    }
}
