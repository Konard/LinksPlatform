using System;
using Sigil;
using Platform.Data.Triplets;

namespace Platform.Sandbox
{
    static class Compilier
    {
        public static Func<Link, bool> CompileSelectionCheck(Action<Emit<Func<Link, bool>>> emit)
        {
            var emitter = Emit<Func<Link, bool>>.NewDynamicMethod();
            emit(emitter);
            return emitter.CreateDelegate();
        }
    }
}