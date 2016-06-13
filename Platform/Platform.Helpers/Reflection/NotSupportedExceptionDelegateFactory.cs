using System;
using Sigil;

namespace Platform.Helpers.Reflection
{
    public class NotSupportedExceptionDelegateFactory<TDelegate> : IFactory<TDelegate>
    {
        public TDelegate Create()
        {
            var emiter = Emit<TDelegate>.NewDynamicMethod();
            emiter.NewObject<NotSupportedException>();
            emiter.Throw();
            return emiter.CreateDelegate();
        }
    }
}
