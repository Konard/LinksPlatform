using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Platform.Sandbox
{
    public abstract class StaticBase<TSuccessor>
            where TSuccessor : StaticBase<TSuccessor>, new()
    {
        protected static readonly TSuccessor Instance = new TSuccessor();
    }

    public class Base : StaticBase<Base>
    {
        public Base()
        {
        }

        public void MethodA()
        {
        }
    }

    public class Inherited : Base
    {
        private Inherited()
        {
        }

        public new static void MethodA()
        {
            Instance.MethodA();
        }
    }

    public static partial class A
    {
        public static void Method1()
        {
        }
    }

    public static partial class A
    {
        public static void Method2()
        {
        }
    }
}
