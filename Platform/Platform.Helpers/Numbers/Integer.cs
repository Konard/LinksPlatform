using System;
using System.Reflection;
using Platform.Helpers.Reflection;
using Platform.Helpers.Reflection.Sigil;

// ReSharper disable StaticFieldInGenericType

namespace Platform.Helpers.Numbers
{
    public struct Integer
    {
        public readonly ulong Value;

        public Integer(ulong value) => Value = value;

        public static implicit operator Integer(ulong integer) => new Integer(integer);

        public static implicit operator Integer(long integer) => To.UInt64(integer);

        public static implicit operator Integer(uint integer) => new Integer(integer);

        public static implicit operator Integer(int integer) => To.UInt64(integer);

        public static implicit operator Integer(ushort integer) => new Integer(integer);

        public static implicit operator Integer(short integer) => To.UInt64(integer);

        public static implicit operator Integer(byte integer) => new Integer(integer);

        public static implicit operator Integer(sbyte integer) => To.UInt64(integer);

        public static implicit operator Integer(bool integer) => To.UInt64(integer);

        public static implicit operator ulong(Integer integer) => integer.Value;

        public static implicit operator long(Integer integer) => To.Int64(integer.Value);

        public static implicit operator uint(Integer integer) => To.UInt32(integer.Value);

        public static implicit operator int(Integer integer) => To.Int32(integer.Value);

        public static implicit operator ushort(Integer integer) => To.UInt16(integer.Value);

        public static implicit operator short(Integer integer) => To.Int16(integer.Value);

        public static implicit operator byte(Integer integer) => To.Byte(integer.Value);

        public static implicit operator sbyte(Integer integer) => To.SByte(integer.Value);

        public static implicit operator bool(Integer integer) => To.Boolean(integer.Value);

        public override string ToString() => Value.ToString();
    }

    public struct Integer<T>
    {
        private static class CompiledFactory
        {
            public static readonly Func<ulong, Integer<T>> Create;

            static CompiledFactory()
            {
                DelegateHelpers.Compile(out Create, emiter =>
                {
                    if (!CachedTypeInfo<T>.CanBeNumeric && typeof(T) != typeof(Integer))
                        throw new NotSupportedException();

                    emiter.LoadArgument(0);

                    if (typeof(T) != typeof(ulong) && typeof(T) != typeof(Integer))
                        emiter.Call(typeof(To).GetTypeInfo().GetMethod(typeof(T).Name, Types<ulong>.Array));

                    if (CachedTypeInfo<T>.IsNullable)
                        emiter.NewObject(typeof(T), CachedTypeInfo<T>.UnderlyingType);

                    if (typeof(T) == typeof(Integer))
                        emiter.NewObject(typeof(Integer), typeof(ulong));

                    emiter.NewObject(typeof(Integer<T>), typeof(T));

                    emiter.Return();
                });
            }
        }

        public static readonly T Zero;
        public static readonly T One;
        public static readonly T Two;

        static Integer()
        {
            try
            {
                Zero = default;
                One = MathHelpers.Increment(Zero);
                Two = MathHelpers.Increment(One);
            }
            catch (Exception exception)
            {
                Global.OnIgnoredException(exception);
            }
        }

        public readonly T Value;

        public Integer(T value) => Value = value;

        public static implicit operator Integer(Integer<T> integer)
        {
            if (typeof(T) == typeof(Integer))
                return (Integer)(object)integer.Value;
            return Convert.ToUInt64(integer.Value);
        }

        public static implicit operator ulong(Integer<T> integer) => ((Integer)integer).Value;

        public static implicit operator T(Integer<T> integer) => integer.Value;

        public static implicit operator Integer<T>(T integer) => new Integer<T>(integer);

        public static implicit operator Integer<T>(ulong integer) => CompiledFactory.Create(integer);

        public static implicit operator Integer<T>(Integer integer) => CompiledFactory.Create(integer.Value);

        public static implicit operator Integer<T>(long integer) => To.UInt64(integer);

        public static implicit operator Integer<T>(uint integer) => new Integer(integer);

        public static implicit operator Integer<T>(int integer) => To.UInt64(integer);

        public static implicit operator Integer<T>(ushort integer) => new Integer(integer);

        public static implicit operator Integer<T>(short integer) => To.UInt64(integer);

        public static implicit operator Integer<T>(byte integer) => new Integer(integer);

        public static implicit operator Integer<T>(sbyte integer) => To.UInt64(integer);

        public static implicit operator Integer<T>(bool integer) => To.UInt64(integer);

        public static implicit operator long(Integer<T> integer) => To.Int64(integer);

        public static implicit operator uint(Integer<T> integer) => To.UInt32(integer);

        public static implicit operator int(Integer<T> integer) => To.Int32(integer);

        public static implicit operator ushort(Integer<T> integer) => To.UInt16(integer);

        public static implicit operator short(Integer<T> integer) => To.Int16(integer);

        public static implicit operator byte(Integer<T> integer) => To.Byte(integer);

        public static implicit operator sbyte(Integer<T> integer) => To.SByte(integer);

        public static implicit operator bool(Integer<T> integer) => To.Boolean(integer);

        public override string ToString() => Value.ToString();
    }
}
