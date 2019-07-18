using System;
using Sigil;
using Platform.Interfaces;

namespace Platform.Helpers.Reflection.Sigil
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
