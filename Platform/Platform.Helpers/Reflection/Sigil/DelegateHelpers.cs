using System;
using System.Collections.Generic;
using Sigil;

namespace Platform.Helpers.Reflection.Sigil
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
                if (EqualityComparer<TDelegate>.Default.Equals(@delegate, default))
                    @delegate = Singleton.Get(Default<NotSupportedExceptionDelegateFactory<TDelegate>>.Instance);
            }
        }
    }
}
