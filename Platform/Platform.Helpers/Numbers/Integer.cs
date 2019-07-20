using System;
using System.Reflection;
using Platform.Reflection;
using Platform.Reflection.Sigil;
using Platform.Converters;


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
}
