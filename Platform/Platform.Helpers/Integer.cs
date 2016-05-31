using System;
using System.Reflection;
using Platform.Helpers.Reflection;

// ReSharper disable StaticFieldInGenericType

namespace Platform.Helpers
{
    public struct Integer
    {
        public readonly ulong Value;

        public Integer(long value)
            : this((ulong)value)
        {
        }

        public Integer(ulong value)
        {
            Value = value;
        }

        public static implicit operator Integer(ulong integer) => new Integer(integer);

        public static implicit operator Integer(long integer) => new Integer(integer);

        public static implicit operator Integer(uint integer) => new Integer(integer);

        public static implicit operator Integer(int integer) => new Integer(integer);

        public static implicit operator Integer(ushort integer) => new Integer(integer);

        public static implicit operator Integer(short integer) => new Integer(integer);

        public static implicit operator Integer(byte integer) => new Integer(integer);

        public static implicit operator Integer(sbyte integer) => new Integer(integer);

        public static implicit operator Integer(bool integer) => new Integer(integer ? 1 : 0);

        public static implicit operator ulong(Integer integer) => To.UInt64(integer.Value);

        public static implicit operator long(Integer integer) => To.Int64(integer.Value);

        public static implicit operator uint(Integer integer) => To.UInt32(integer.Value);

        public static implicit operator int(Integer integer) => To.Int32(integer.Value);

        public static implicit operator ushort(Integer integer) => To.UInt16(integer.Value);

        public static implicit operator short(Integer integer) => To.Int16(integer.Value);

        public static implicit operator byte(Integer integer) => To.Byte(integer.Value);

        public static implicit operator sbyte(Integer integer) => To.SByte(integer.Value);

        public static implicit operator bool(Integer integer) => To.Boolean(integer.Value);
    }

    public struct Integer<T>
    {
        private static class Factory
        {
            public static readonly Func<ulong, Integer<T>> Create;

            static Factory()
            {
                try
                {
                    if (CachedTypeInfo<T>.CanBeNumeric || typeof(T) == typeof(Integer))
                    {
                        var emiter = Sigil.Emit<Func<ulong, Integer<T>>>.NewDynamicMethod();

                        emiter.LoadArgument(0);

                        if (typeof(T) != typeof(ulong) && typeof(T) != typeof(Integer))
                            emiter.Call(typeof(To).GetMethod(typeof(T).Name, Types<ulong>.Array));

                        if (CachedTypeInfo<T>.IsNullable)
                            emiter.NewObject(typeof(T), CachedTypeInfo<T>.UnderlyingType);

                        if (typeof(T) == typeof(Integer))
                            emiter.NewObject(typeof(Integer), typeof(ulong));

                        emiter.NewObject(typeof(Integer<T>), typeof(T));

                        emiter.Return();

                        Create = emiter.CreateDelegate();
                    }
                }
                catch (Exception exception)
                {
                    Global.OnIgnoredException(exception);
                }
                finally
                {
                    if (Create == null)
                        Create = arg => { throw new NotSupportedException(); };
                }
            }
        }

        public readonly T Value;

        public Integer(T value)
        {
            Value = value;
        }

        public static implicit operator Integer(Integer<T> integer)
        {
            if (typeof(T) == typeof(Integer))
                return (Integer)(object)integer.Value;
            return Convert.ToUInt64(integer.Value);
        }

        public static implicit operator ulong(Integer<T> integer) => ((Integer)integer).Value;

        public static implicit operator T(Integer<T> integer) => integer.Value;

        public static implicit operator Integer<T>(T integer) => new Integer<T>(integer);

        public static implicit operator Integer<T>(ulong integer) => Factory.Create(integer);

        public static implicit operator Integer<T>(Integer integer) => Factory.Create(integer.Value);
    }
}
