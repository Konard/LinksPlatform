using System;
using Sigil;

namespace Platform.Helpers.Reflection
{
    public static class SigilExtensions
    {
        public static Emit<TDelegate> LoadConstantOne<TDelegate>(this Emit<TDelegate> emiter, Type constantType)
        {
            if (constantType == typeof(float))
                emiter.LoadConstant(1F);
            else if (constantType == typeof(double))
                emiter.LoadConstant(1D);
            else if (constantType == typeof(long))
                emiter.LoadConstant(1L);
            else if (constantType == typeof(ulong))
                emiter.LoadConstant(1UL);
            else if (constantType == typeof(int))
                emiter.LoadConstant(1);
            else if (constantType == typeof(uint))
                emiter.LoadConstant(1U);
            else if (constantType == typeof(short))
            {
                emiter.LoadConstant(1);
                emiter.Convert<short>();
            }
            else if (constantType == typeof(ushort))
            {
                emiter.LoadConstant(1);
                emiter.Convert<ushort>();
            }
            else if (constantType == typeof(sbyte))
            {
                emiter.LoadConstant(1);
                emiter.Convert<sbyte>();
            }
            else if (constantType == typeof(byte))
            {
                emiter.LoadConstant(1);
                emiter.Convert<byte>();
            }
            else
                throw new NotSupportedException();
            return emiter;
        }

        public static Emit<TDelegate> LoadConstant<TDelegate>(this Emit<TDelegate> emiter, Type constantType, object constantValue)
        {
            if (constantType == typeof(float))
                emiter.LoadConstant((float)constantValue);
            else if (constantType == typeof(double))
                emiter.LoadConstant((double)constantValue);
            else if (constantType == typeof(long))
                emiter.LoadConstant((long)constantValue);
            else if (constantType == typeof(ulong))
                emiter.LoadConstant((ulong)constantValue);
            else if (constantType == typeof(int))
                emiter.LoadConstant((int)constantValue);
            else if (constantType == typeof(uint))
                emiter.LoadConstant((uint)constantValue);
            else if (constantType == typeof(short))
            {
                emiter.LoadConstant((short)constantValue);
                emiter.Convert<short>();
            }
            else if (constantType == typeof(ushort))
            {
                emiter.LoadConstant((ushort)constantValue);
                emiter.Convert<ushort>();
            }
            else if (constantType == typeof(sbyte))
            {
                emiter.LoadConstant((sbyte)constantValue);
                emiter.Convert<sbyte>();
            }
            else if (constantType == typeof(byte))
            {
                emiter.LoadConstant((byte)constantValue);
                emiter.Convert<byte>();
            }
            else
                throw new NotSupportedException();
            return emiter;
        }

        public static Emit<TDelegate> Increment<TDelegate>(this Emit<TDelegate> emiter, Type valueType)
        {
            emiter.LoadConstantOne(valueType);
            emiter.Add();
            return emiter;
        }

        public static Emit<TDelegate> Decrement<TDelegate>(this Emit<TDelegate> emiter, Type valueType)
        {
            emiter.LoadConstantOne(valueType);
            emiter.Subtract();
            return emiter;
        }

        public static Emit<TDelegate> LoadArguments<TDelegate>(this Emit<TDelegate> emiter, params ushort[] arguments)
        {
            for (var i = 0; i < arguments.Length; i++)
                emiter.LoadArgument(arguments[i]);
            return emiter;
        }

        public static Emit<TDelegate> CompareGreaterThan<TDelegate>(this Emit<TDelegate> emiter, bool isSigned)
        {
            if (isSigned)
                emiter.CompareGreaterThan();
            else
                emiter.UnsignedCompareGreaterThan();
            return emiter;
        }

        public static Emit<TDelegate> CompareLessThan<TDelegate>(this Emit<TDelegate> emiter, bool isSigned)
        {
            if (isSigned)
                emiter.CompareLessThan();
            else
                emiter.UnsignedCompareLessThan();
            return emiter;
        }
    }
}
